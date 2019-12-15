using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using VideoLibrary;


namespace Soundscripter.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private string vidName = "mp3ToScript";

        [BindProperty]
        public string YoutubeUrl { get; set; } = "";

        [Required]
        [Display(Name="Speaker Name")]
        public string SpeakerName { get; set; }
        public IEnumerable<SelectListItem> Speakers { get; set; }

        public byte[] AudioBytesArray { get; set; }
        public string Message { get; set; } = "PLACE FOR TRANSCRIPT";
        public RecognitionResponseProcessor RecognitionResponseProcessor { get; set; } = new RecognitionResponseProcessor();


        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem("test", "testowy"));
            selectListItems.Add(new SelectListItem("test1", "testowy1"));
            selectListItems.Add(new SelectListItem("test2", "testowy1"));
            Speakers = selectListItems;
        }

        public void OnGet()
        {

        }
        public async Task OnPost()
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
            //System.IO.File.ReadAllBytes();
            await Transcript(outputFile.Filename);
        }

        private async Task Transcript(string sourceUri = "C:\\Users\\Mateusz.Galasinski\\Desktop\\debate_test.mp3")
        {
            var buckerLoader = new BucketLoader();
            (string audioInBucketUri, string objectName) = buckerLoader.UploadFileFromLocal(sourceUri);
            SpeechTranscripter transcripter = new SpeechTranscripter();
            LongRunningRecognizeResponse response = await transcripter.Recognize(audioInBucketUri, new RecognizeConfiguration());
            buckerLoader.DeleteObject(new[] { objectName });

            string transcriptId = Guid.NewGuid().ToString().Substring(0, 10);
            await RecognitionResponseProcessor.FindSamples(transcriptId, response);
            Message = JsonSerializer.Serialize(
                new
                {
                    transcriptId,
                    response
                });
        }
    }
}
