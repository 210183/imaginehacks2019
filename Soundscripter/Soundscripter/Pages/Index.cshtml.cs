using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public byte[] AudioBytesArray { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
        public void OnPost()
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
        }
    }
}
