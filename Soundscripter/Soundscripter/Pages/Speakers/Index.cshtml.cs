using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Soundscripter.Mongo;

namespace Soundscripter.Pages.Speakers
{
    public class IndexModel : PageModel
    {
        [Required]
        [Display(Name = "Speaker Name")]
        public string SpeakerName { get; set; }
        public IEnumerable<SelectListItem> Speakers { get; set; }

        public IndexModel()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem("test", "testowy"));
            selectListItems.Add(new SelectListItem("test1", "testowy1"));
            selectListItems.Add(new SelectListItem("test2", "testowy1"));
            Speakers = selectListItems;
        }



        public async Task<IActionResult> OnGet()
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECT_STR");
            var database = CosmosUtils.ConnectToDatabase(connectionString, "Samples");
            var collection = database.GetCollection<SamplesCollection>("Samples");

            var speakers = new List<string>();
            foreach (var samplesEntity in await CosmosUtils.GetAllAsync(collection))
            {
                speakers.AddRange(samplesEntity.samples.Select(s => s.speakerName).Distinct().ToList());
            }

            Speakers = speakers.Distinct().Select(s => new SelectListItem(s, s)).ToList();


            return Page();
        }

        public async Task<JsonResult> OnGetSelectedSpeaker(string speakerName)
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECT_STR");
            var database = CosmosUtils.ConnectToDatabase(connectionString, "Samples");
            var collection = database.GetCollection<SamplesCollection>("Samples");

            var transcripts = new List<Transcript>();
            foreach (var samplesCollection in await CosmosUtils.GetAllAsync(collection))
            {
                var transcriptParts = samplesCollection.samples.Where(s => s.speakerName == speakerName)
                    .Select(s => new Part()
                    {
                        Text = s.text, 
                        Time = $"{s.duration}",
                        StartTime = s.startTime,
                        EndTime = s.endTime
                    }).ToList();
                if (transcriptParts.Count > 0)
                {
                    transcripts.Add(new Transcript()
                    {
                        TranscriptId = samplesCollection.transcriptId,
                        VideoUri = samplesCollection.VideoUri,
                        Parts = transcriptParts
                    });
                }

            }

            return new JsonResult(transcripts);
        }
    }


    public class Transcript
    {
        public string TranscriptId { get; set; }
        public string VideoUri { get; set; }
        public List<Part> Parts { get; set; }
    }

    public class Part
    {
        public string Text { get; set; }
        public string Time { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}