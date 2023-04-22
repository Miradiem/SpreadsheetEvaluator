using SpreadsheetEvaluator.Api;

namespace SpreadsheetEvaluator.App
{
    public class Submission
    {
        private readonly string _baseUrl;
        private readonly string _email;
        
        public Submission(string baseUrl, string email)
        {
            _baseUrl = baseUrl;
            _email = email;
        }

        public async Task<string> SubmitEvaluationResult()
        {
            var api = new SheetsApi(
                new ApiClient(_baseUrl)
                .Create());

            var spreadSheet = await api.GetSheets("sheets");
            var submissionUrl = spreadSheet.SubmissionUrl.Replace(_baseUrl, "");

            var sheetData = Functions.CloneSheetData(spreadSheet);
            sheetData.ForEach(sheet => new Evaluation().EvaluateSpreadsheet(sheet));

            var submissionResult = new SubmissionResult()
            {
                Email = _email,
                Results = sheetData
            };
           
            return await api.PostSubmissions(submissionUrl, submissionResult);
        }
    }
}
