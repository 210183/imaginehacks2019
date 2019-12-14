using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Soundscripter.Pages
{
    public class TranscriptModel : PageModel
    {
        public string Message { get; set; } = "PLACE FOR TRANSCRIPT";

        public void OnGet()
        {

        }

        public async Task OnPostTranscript(string sourceUri = "C:\\Users\\Mateusz.Galasinski\\Desktop\\debate_test.mp3")
        {
            var buckerLoader = new BucketLoader();
            (string audioInBucketUri, string objectName) = buckerLoader.UploadFileFromLocal(sourceUri);
            SpeechTranscripter transcripter = new SpeechTranscripter();
            LongRunningRecognizeResponse response = await transcripter.Recognize(audioInBucketUri, new RecognizeConfiguration());
            buckerLoader.DeleteObject(new []{ objectName });
            Message = JsonSerializer.Serialize(response);
        }
    }
}