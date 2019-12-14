using System;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;

namespace Soundscripter
{
    public class RecognizeConfiguration
    {
        public int SampleRateHertz { get; set; } = 8000;
        public int MinSpeakerCount { get; set; } = 1;
        public int MaxSpeakerCount { get; set; } = 10;
        public string LanguageCode { get; set; } = "en";
        public RecognitionConfig.Types.AudioEncoding AudioEncoding { get; set; } = RecognitionConfig.Types.AudioEncoding.EncodingUnspecified;
    }

    public class SpeechTranscripter
    {
        public async Task<LongRunningRecognizeResponse> Recognize(string storageUri, RecognizeConfiguration configuration = null)
        {
            configuration ??= new RecognizeConfiguration();
            
            var speech = SpeechClient.Create();
            var audio = RecognitionAudio.FromStorageUri(storageUri);
            var longOperation = await speech.LongRunningRecognizeAsync(new RecognitionConfig()
            {
                Encoding = configuration.AudioEncoding,
                SampleRateHertz = configuration.SampleRateHertz,
                LanguageCode = configuration.LanguageCode,
                DiarizationConfig = new SpeakerDiarizationConfig()
                {
                    EnableSpeakerDiarization = true,
                    MinSpeakerCount = configuration.MinSpeakerCount,
                    MaxSpeakerCount = configuration.MaxSpeakerCount
                },
                Metadata = new RecognitionMetadata()
                {
                    OriginalMediaType = RecognitionMetadata.Types.OriginalMediaType.Video
                }
                //}, RecognitionAudio.FromStorageUri(storageUri));
            }, audio);
            //}, RecognitionAudio.FetchFromUri("https://www.youtube.com/watch?v=5Btbdt7ksko&fbclid=IwAR2FQ5KlTzxHH7UdYDTx4Vcnk6TfFfFtWpMJw-jH1UOMAbodsnY8mS1bNlI"));
            longOperation = await longOperation.PollUntilCompletedAsync();
            LongRunningRecognizeResponse response = longOperation.Result;
            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    Console.WriteLine($"Transcript: { alternative.Transcript}");
                }
            }
            return response;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
