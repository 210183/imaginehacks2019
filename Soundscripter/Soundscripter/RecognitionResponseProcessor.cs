using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using Google.Protobuf.WellKnownTypes;
using NAudio.Wave;
using Soundscripter.Mongo;

namespace Soundscripter
{
    public class RecognitionResponseProcessor
    {
        public BlobStorageLoader StorageLoader { get; set; } = new BlobStorageLoader();
        public AudioTrimmer AudioTrimmer { get; set; } = new AudioTrimmer();
        public string SourceAudioFile { get; set; } = "C:\\Users\\Mateusz.Galasinski\\Desktop\\debate_test.mp3";

        public async Task FindSamples(string transcriptId, LongRunningRecognizeResponse recognizeResponse)
        {
            var result = recognizeResponse.Results.Last();
            if (result == null)
            {
                throw new ArgumentException("Empty recognition response. Cannot find samples.");
            }

            var words = result.Alternatives.Last().Words;
            int currentSpeakerTag = -1;
            List<WordInfo> currentSampleWords = new List<WordInfo>();
            List<Sample> samples = new List<Sample>();
            foreach (WordInfo wordInfo in words)
            {
                if (currentSpeakerTag == -1)
                {
                    currentSpeakerTag = wordInfo.SpeakerTag;
                    currentSampleWords.Add(wordInfo);
                }
                else
                {
                    if (currentSpeakerTag != wordInfo.SpeakerTag) // save new sample
                    {
                        await AddSample();

                        //switch speaker
                        currentSampleWords.Clear();
                        currentSpeakerTag = wordInfo.SpeakerTag;
                    }
                    else
                    {
                        currentSampleWords.Add(wordInfo);
                    }
                }
            }

            //last sample
            await AddSample();

            var samplesToSave = new SamplesCollection()
            {
                samples = samples,
                transcriptId = transcriptId
            };

            var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECT_STR");
            var database = CosmosUtils.ConnectToDatabase(connectionString, "Samples");
            var collection = database.GetCollection<SamplesCollection>("Samples");
            await CosmosUtils.AddDocumentAsync(collection, samplesToSave);
            foreach (var invoiceEntity in await CosmosUtils.GetAllAsync(collection))
            {
                Console.WriteLine(invoiceEntity.transcriptId);
            }

            async Task AddSample()
            {
                var orderedWords = currentSampleWords.OrderBy(w => w.StartTime.Nanos);
                var firstWord = orderedWords.First();
                var lastWord = orderedWords.Last();
                Duration duration = orderedWords.Last().EndTime - orderedWords.First().StartTime;
                string trimmedFile = AudioTrimmer.SaveTrimmed(
                    (int)(firstWord.StartTime.Seconds * 1000) + firstWord.StartTime.Nanos / 1000_000,
                    (int)(lastWord.EndTime.Seconds * 1000) + lastWord.EndTime.Nanos / 1000_000,
                    SourceAudioFile);
                string blobName = await StorageLoader.PutIntoBlob(trimmedFile);
                samples.Add(new Sample()
                {
                    duration = (int)(duration.Seconds * 1000) + duration.Nanos / 1000_000,
                    wordCount = orderedWords.Count(),
                    speakerId = currentSpeakerTag,
                    storageUri = $"{StorageLoader.BlobServiceClient.Uri}{StorageLoader.BlobName}/{blobName}",
                    text = string.Join(' ', currentSampleWords.Select(w => w.Word))
                });
            }
        }
    }

    public class AudioTrimmer
    {
        public string SaveTrimmed(int startMiliseconds, int endMiliseconds, string sourceAudioFile)
        {
            var mp3Path = sourceAudioFile;
            string path = Path.ChangeExtension(mp3Path, "");
            path = path + Guid.NewGuid().ToString().Substring(0, 8);
            var outputPath = Path.ChangeExtension(path, ".trimmed.mp3");

            TrimMp3(mp3Path, outputPath, TimeSpan.FromMilliseconds(startMiliseconds), TimeSpan.FromMilliseconds(endMiliseconds));

            return outputPath;
        }

        private void TrimMp3(string inputPath, string outputPath, TimeSpan? begin, TimeSpan? end)
        {
            if (begin.HasValue && end.HasValue && begin > end)
                throw new ArgumentOutOfRangeException("end", "end should be greater than begin");

            using (var reader = new Mp3FileReader(inputPath))
            using (var writer = File.Create(outputPath))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                    if (reader.CurrentTime >= begin || !begin.HasValue)
                    {
                        if (reader.CurrentTime <= end || !end.HasValue)
                            writer.Write(frame.RawData, 0, frame.RawData.Length);
                        else break;
                    }
            }
        }
    }
}
