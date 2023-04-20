using SpreadsheetEvaluator.Api;
using SpreadsheetEvaluator.App;

var email = "kipras.st@gmail.com";
var baseApiUrl = "https://www.wix.com/_serverless/hiring-task-spreadsheet-evaluator";
var sheetsApi = new SheetsApi(
    new ApiClient(baseApiUrl).Create());
var spreadSheet = await sheetsApi.GetSheets("sheets");
var submissionUrl = spreadSheet.SubmissionUrl.Replace(baseApiUrl, "");

var sheetList = spreadSheet.Sheets;
sheetList.ForEach(sheet => new Evaluation().EvaluateSpreadsheet(sheet));

var submissionResult = new SubmissionResult()
{
    Email = email,
    Results = sheetList
};

var postResult = await sheetsApi.PostSubmissions(submissionUrl, submissionResult);

Console.WriteLine(postResult);





