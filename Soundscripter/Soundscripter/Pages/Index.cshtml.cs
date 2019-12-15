using System;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using VideoLibrary;

namespace Soundscripter.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private string vidName = "mp3ToScript";

        [BindProperty]
        public string YoutubeUrl { get; set; } = "";

        public byte[] AudioBytesArray { get; set; }
        public string Message { get; set; } = "PLACE FOR TRANSCRIPT";
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

            var inputFile = new MediaFile { Filename = vidName };
            var outputFile = new MediaFile { Filename = $"{vidName}.mp3" };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);

                engine.Convert(inputFile, outputFile);
            }
            string id = await Transcript(outputFile.Filename);
            return RedirectToPage("./Samples/Index", new { transcriptId = id });
        }

        private async Task<string> Transcript(string sourceUri = "C:\\Users\\Mateusz.Galasinski\\Desktop\\debate_test.mp3")
        {
            var buckerLoader = new BucketLoader();
            (string audioInBucketUri, string objectName) = buckerLoader.UploadFileFromLocal(sourceUri);
            SpeechTranscripter transcripter = new SpeechTranscripter();
            LongRunningRecognizeResponse response = await transcripter.Recognize(audioInBucketUri, new RecognizeConfiguration());
            buckerLoader.DeleteObject(new[] { objectName });

            string transcriptId = ObjectId.GenerateNewId().ToString();
            await RecognitionResponseProcessor.FindSamples(transcriptId, response);
            Message = JsonSerializer.Serialize(
                new
                {
                    transcriptId,
                    response
                });
            return transcriptId;
        }
    }
}
