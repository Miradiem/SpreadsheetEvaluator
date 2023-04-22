using FluentAssertions;
using Flurl.Http.Testing;
using SpreadsheetEvaluator.Api;
using SpreadsheetEvaluator.App;
using System.Threading.Tasks;
using Xunit;

namespace SpreadsheetEvaluator.Tests.App
{
    public class EvalutionCommandTests
    {
        [Fact]
        public async Task ShouldSubmitEvaluationResult()
        {
            using (var httpTest = new HttpTest())
            {
                var spreadsheet = new Spreadsheet();
                spreadsheet.SubmissionUrl = "https://testingSubmissionUrl.com";
                spreadsheet.Sheets.Add(
                    new SheetData()
                    {
                        Id = "sheet-1",
                        Data = new object[][]
                        {
                            new object[] { 1 },
                            new object[] { "A1" },
                            new object[] { true }
                        }
                    });

                httpTest
                  .ForCallsTo("https://testingcall.com/sheets")
                  .RespondWithJson(spreadsheet);

                httpTest
                   .ForCallsTo("")
                   .RespondWithJson(
                    new ResponseMessage()
                    {
                        Message = "Great success!"
                    });

                var sut = CreateSut();
                var result = await sut.SubmitEvaluationResult();

                result.Should().Be("Great success!");
            }
        }

        private EvaluationCommand CreateSut() =>
            new EvaluationCommand("https://testingcall.com", "test@email.com");
    }
}