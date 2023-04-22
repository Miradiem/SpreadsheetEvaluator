using SpreadsheetEvaluator.Api;

namespace SpreadsheetEvaluator.App
{
    public class EvaluationCommand
    {
        private readonly string _baseUrl;
        private readonly string _email;
        
        public EvaluationCommand(string baseUrl, string email)
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

            var sheetData = SpreadSheetFunctions.CloneSheetData(spreadSheet.Sheets);
            sheetData.ForEach(sheet => new Evaluation().EvaluateSpreadsheet(sheet));

            var submissionResult = new SubmissionResult()
            {
                Email = _email,
                Results = sheetData
            };
           
            return await api.PostSubmissions(spreadSheet.SubmissionUrl.Replace(_baseUrl, ""), submissionResult);
        }
    }
}