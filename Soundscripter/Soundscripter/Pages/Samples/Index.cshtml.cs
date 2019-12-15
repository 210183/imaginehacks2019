using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soundscripter.Mongo;

namespace Soundscripter.Pages
{
    public class SamplesModel : PageModel
    {
        [BindProperty]
        public List<SampleDto> Samples { get; set; }

        public async Task OnGet(string? transcriptId)
        {
            var samples = await LoadSamples(transcriptId);
            var longestSamples = samples.samples
                .GroupBy(kv => kv.speakerId)
                .Select(g => new
                {
                    group = g,
                    max = g.Max(s => s.wordCount)
                })
                .Select(g => g.group.First(s => s.wordCount == g.max));

            Samples = longestSamples.Select(s => new SampleDto()
            {
                SpeakerId = s.speakerId,
                AudioUrl = s.storageUri
            }).ToList();
        }

        private async Task<SamplesCollection> LoadSamples(string transcriptId)
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECT_STR");
            var database = CosmosUtils.ConnectToDatabase(connectionString, "Samples");
            var collection = database.GetCollection<SamplesCollection>("Samples");

            foreach (var samplesEntity in await CosmosUtils.GetAllAsync(collection))
            {
                if (samplesEntity.transcriptId == transcriptId)
                {
                    return samplesEntity;
                }
            }

            throw new ArgumentException($"Samples not found: {transcriptId}");
        }

        public async Task<IActionResult> OnPost(string? transcriptId, List<SampleDto> samples)
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECT_STR");
            var database = CosmosUtils.ConnectToDatabase(connectionString, "Samples");
            var collection = database.GetCollection<SamplesCollection>("Samples");
            SamplesCollection samplesCollection = null;
            foreach (var samplesEntity in await CosmosUtils.GetAllAsync(collection))
            {
                if (samplesEntity.transcriptId == transcriptId)
                {
                    samplesCollection = samplesEntity;
                    break;
                }
            }

            foreach (var classifiedSample in samples)
            {
                foreach (var sample in samplesCollection.samples
                    .Where(s => s.speakerId == classifiedSample.SpeakerId))
                {
                    sample.speakerName = classifiedSample.SpeakerName;
                }
            }
            await CosmosUtils.UpdateDocumentAsync(collection, samplesCollection);
            return RedirectToPage("../Speakers/Index");
        }
    }

    public class SampleDto
    {
        public int SpeakerId { get; set; }
        public string SpeakerName { get; set; }
        public string AudioUrl { get; set; }
    }
}
