using FluentAssertions;
using Flurl.Http.Testing;
using SpreadsheetEvaluator.Api;
using System.Linq;
using Xunit;

namespace SpreadsheetEvaluator.Tests.Api
{
    public class SheetsApiTests
    {
        [Fact]
        public void ShouldGetSheets()
        {
            using (var httpTest = new HttpTest())
            {
                var sheetData = new object[][]
                {
                    new object[] { 1 },
                    new object[] { "A1" },
                    new object[] { true }
                };

                var spreadsheet = new Spreadsheet();
                spreadsheet.SubmissionUrl = "https://testingSubmissionUrl.com";
                spreadsheet.Sheets.Add(
                    new SheetData()
                    {
                        Id = "sheet-1",
                        Data = sheetData
                    });

                httpTest
                    .ForCallsTo("https://testingcall.com/test")
                    .RespondWithJson(spreadsheet);

                var sut = CreateSut();
                var result = sut.GetSheets("test");

                
                result.Result.SubmissionUrl.Should().Be("https://testingSubmissionUrl.com");
                result.Result.Sheets.Should().Contain(sheet => sheet.Id == "sheet-1");
                result.Result.Sheets.SelectMany(sheet => sheet.Data).Should().BeEquivalentTo(sheetData);
            }
        }

        private SheetsApi CreateSut() =>
             new SheetsApi(new ApiClient("https://testingcall.com"));
        
    }
}
