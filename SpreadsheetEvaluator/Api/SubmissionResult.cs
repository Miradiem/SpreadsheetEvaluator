using Newtonsoft.Json;

namespace SpreadsheetEvaluator.Api
{
    public class SubmissionResult
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("results")]
        public List<SheetData> Results { get; set; } = new List<SheetData>();
    }
}