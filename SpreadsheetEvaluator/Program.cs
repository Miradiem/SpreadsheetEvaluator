using SpreadsheetEvaluator.App;

 var submission = await new EvaluationCommand("https://www.wix.com/_serverless/hiring-task-spreadsheet-evaluator", "kipras.st@gmail.com")
    .SubmitEvaluationResult();

 Console.WriteLine(submission);