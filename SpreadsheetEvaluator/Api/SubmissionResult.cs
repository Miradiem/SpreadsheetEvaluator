using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
