using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetEvaluator.Api
{
    public class Token
    {
        [JsonProperty("message")]
        public string Message { get; set; }  
    }
}
