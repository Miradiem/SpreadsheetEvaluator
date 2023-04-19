using SpreadsheetEvaluator.Api;
using SpreadsheetEvaluator.App;
using System.Data;

var baseApiUrl = "https://www.wix.com/_serverless/hiring-task-spreadsheet-evaluator";
var sheetsApi = new SheetsApi(
        new ApiClient(baseApiUrl).Create());

var spreadSheet = await sheetsApi.GetSheets("sheets");
var submissionUrl = spreadSheet.SubmissionUrl.Replace(baseApiUrl, "");
var sheetList = spreadSheet.Sheets;

var evaluation = new Evaluation();
foreach (var sheet in sheetList)
{
    evaluation.EvaluateSpreadsheet(sheet);
    
}

var submissionResult = new SubmissionResult()
{
    Email = "kipras.st@gmail.com",
    Results = sheetList
};

foreach (var item in submissionResult.Results)
{
    Console.WriteLine(item.Id);
    Console.WriteLine(item.Data.Count() + " THAT MANY OBJECTS[][] IN SHEET-DATA");
    Console.WriteLine(item.Data.Length + " LENGTH OF SHEETDATA");
    foreach (var item2 in item.Data)
    {
        Console.WriteLine(item2.Count() + " THAT MANY OBJECTS in OBJECT[]");

        foreach (var item3 in item2)
        {
            Console.WriteLine(item3);
        }
        
    }
    Console.WriteLine("////////////////");
}

var postResult = await sheetsApi.PostSubmissions(submissionUrl, submissionResult);



Console.WriteLine(postResult);





