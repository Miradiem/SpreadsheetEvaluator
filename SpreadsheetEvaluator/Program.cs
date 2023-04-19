using SpreadsheetEvaluator.Api;
using SpreadsheetEvaluator.App;

var baseApiUrl = "https://www.wix.com/_serverless/hiring-task-spreadsheet-evaluator";
var sheetsApi = new SheetsApi(
    new ApiClient(baseApiUrl).Create());

var spreadSheet = await sheetsApi.GetSheets("sheets");

var submissionUrl = spreadSheet.SubmissionUrl.Replace(baseApiUrl, "");
var sheetList = spreadSheet.Sheets;
spreadSheet.Sheets.ForEach(sheet => new Evaluation().EvaluateSpreadsheet(sheet));

var submissionResult = new SubmissionResult()
{
    Email = "kipras.st@gmail.com",
    Results = sheetList
};
var postResult = await sheetsApi.PostSubmissions(submissionUrl, submissionResult);
Console.WriteLine(postResult);





