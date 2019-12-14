using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Soundscripter.Pages
{
    public class SamplesModel : PageModel
    {
        [BindProperty]
        public List<SampleDto> Samples { get; set; }

        public void OnGet(string transcriptId)
        {
            //TODO findSamples for transcript

            Samples = new List<SampleDto>()
            {
                new SampleDto()
                {
                    SpeakerId = 0,
                    SpeakerName = "",
                    AudioUrl = @"https://soundscripter.blob.core.windows.net/testing-accesss/debate_test.mp3"
                },
                new SampleDto()
                {
                    SpeakerId = 1,
                    SpeakerName = "",
                    AudioUrl = @"https://soundscripter.blob.core.windows.net/testing-accesss/debate_test.mp3"
                }
            };
        }
    }

    public class SampleDto
    {
        public int SpeakerId { get; set; }
        public string SpeakerName { get; set; }
        public string AudioUrl { get; set; }
    }
}
