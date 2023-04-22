namespace SpreadsheetEvaluator.Api
{
    public class Spreadsheet
    {
        public string SubmissionUrl { get; set; }

        public List<SheetData> Sheets { get; set; } = new List<SheetData>();
    }
}