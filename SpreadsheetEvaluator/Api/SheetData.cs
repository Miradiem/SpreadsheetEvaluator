using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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