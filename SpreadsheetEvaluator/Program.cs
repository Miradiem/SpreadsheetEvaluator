using SpreadsheetEvaluator.App;

var baseUrl = "https://www.wix.com/_serverless/hiring-task-spreadsheet-evaluator";
var email = "kipras.st@gmail.com";

var submission = await new Submission(baseUrl, email).SubmitEvaluationResult();

Console.WriteLine(submission);
