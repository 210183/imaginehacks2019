using System;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using VideoLibrary;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Enums;
using Xabe.FFmpeg.Model;


namespace Soundscripter.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private string vidName = "mp3ToScript";

        [BindProperty]
        public string YoutubeUrl { get; set; } = "";

        public byte[] AudioBytesArray { get; set; }
        public RecognitionResponseProcessor RecognitionResponseProcessor { get; set; } = new RecognitionResponseProcessor();


        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPost()
        {
            var source = Environment.CurrentDirectory;
            var youtube = YouTube.Default;
            var vid = youtube.GetVideo(YoutubeUrl);
            System.IO.File.WriteAllBytes(vidName, vid.GetBytes());

            //var inputFile = new MediaFile { Filename = vidName };
            //var outputFile = new MediaFile { Filename = $"{vidName}.mp3" };

            //using (var engine = new Engine())
            //{
            //    engine.GetMetadata(inputFile);

            //    engine.Convert(inputFile, outputFile);
            //}


            //var ffmpeg = new NReco.VideoConverter.FFMpegConverter();
            //ffmpeg.FFMpegExeName = "ffmpeg"; // for Linux/OS-X: "ffmpeg"
            //ffmpeg.FFMpegToolPath = "/usr/bin/";
            //ffmpeg.ConvertMedia(vidName, "mp4", $"{vidName}.mp3", null, new ConvertSettings());
            ////var Convert = new NReco.VideoConverter.FFMpegConverter();
            ////string SaveMP3File = Path.ChangeExtension(vidName, ".mp3");
            ////Convert.ConvertMedia(vidName, SaveMP3File, "mp3");
            ////string id = await Transcript(YoutubeUrl, $"{vidName}.mp3");
            string output = Path.ChangeExtension(vidName, FileExtensions.Mp3);
            IConversionResult result = await Conversion.ExtractAudio(vidName, output).Start();

            string id = await Transcript(YoutubeUrl, output);
            //string id = await Transcript(YoutubeUrl, outputFile.Filename);
            return RedirectToPage("./Samples/Index", new { transcriptId = id });
        }

        private async Task<string> Transcript(string originUri, string sourceUri)
        {
            var buckerLoader = new BucketLoader();
            (string audioInBucketUri, string objectName) = buckerLoader.UploadFileFromLocal(sourceUri);
            SpeechTranscripter transcripter = new SpeechTranscripter();
            LongRunningRecognizeResponse response = await transcripter.Recognize(audioInBucketUri, new RecognizeConfiguration());
            buckerLoader.DeleteObject(new[] { objectName });

            string transcriptId = ObjectId.GenerateNewId().ToString();
            await RecognitionResponseProcessor.FindSamples(transcriptId, response, sourceUri, originUri);
            return transcriptId;
        }
    }
}
