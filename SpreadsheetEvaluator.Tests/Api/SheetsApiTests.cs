using FluentAssertions;
using Flurl.Http.Testing;
using SpreadsheetEvaluator.Api;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SpreadsheetEvaluator.Tests.Api
{
    public class SheetsApiTests
    {
        [Fact]
        public async Task ShouldGetSheets()
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
                    .ForCallsTo("https://testingcall.com/test")
                    .RespondWithJson(spreadsheet);

                var sut = CreateSut();
                var result = await sut.GetSheets("test");

                result.SubmissionUrl.Should().Be("https://testingSubmissionUrl.com");
                result.Sheets.Should().Contain(sheet => sheet.Id == "sheet-1");
                result.Sheets.SelectMany(sheet => sheet.Data)
                    .Should().BeEquivalentTo(
                    spreadsheet.Sheets.SelectMany(sheet => sheet.Data));
            }
        }

        [Fact]
        public async Task ShouldPostSubmission()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest
                   .ForCallsTo("https://testingcall.com/testPost")
                   .RespondWithJson(
                    new ResponseMessage()
                    {
                        Message = "Great success!"
                    });

                var sut = CreateSut();
                var result = await sut.PostSubmissions("testPost", new SubmissionResult());

                result.Should().Be("Great success!");
            }
        }

        private SheetsApi CreateSut() =>
             new SheetsApi(new ApiClient("https://testingcall.com").Create());
        
    }
}