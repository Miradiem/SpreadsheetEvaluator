using Newtonsoft.Json;

namespace SpreadsheetEvaluator.Api
{
    public class SheetData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public object[][] Data { get; set; }
    }
}