using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetEvaluator.Api
{
    public class Spreadsheet
    {
        public string SubmissionUrl { get; set; }

        public List<SheetData> Sheets { get; set; } = new List<SheetData>();
    }
}
