using FluentAssertions;
using SpreadsheetEvaluator.Api;
using SpreadsheetEvaluator.App;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpreadsheetEvaluator.Tests.App
{
    public class SpreadSheetFunctionsTests
    {
        [Fact]
        public void ShouldCloneSheetData()
        {
            var testData = new List<SheetData>()
            {
                 new SheetData()
                 {
                     Id = "sheet-1",
                     Data = new object[][]
                     {
                             new object[] { 1 },
                             new object[] { "A1" },
                             new object[] { true }
                     }
                 }
            };

            var result = SpreadSheetFunctions.CloneSheetData(testData);

            result.Should().BeEquivalentTo(testData);
        }

        [Fact]
        public void ShouldCallFunction()
        {
            var testData = new Dictionary<string, List<object>>
            {
                { "A1", new List<object> { 1 } },
                { "SUM", new List<object> { 1.0, 2.0 } },
                { "MULTIPLY", new List<object> { 1.0, 2.0 } },
                { "DIVIDE", new List<object> { 6.0, 3.0 } },
                { "GT", new List<object> { 3.0, 2.0 } },
                { "EQ", new List<object> { 3.0, 3.0 } },
                { "NOT", new List<object> { false } },
                { "AND", new List<object> { true, false } },
                { "OR", new List<object> { true, false } },
                { "IF", new List<object> { true, "Yes", "No" } },
                { "CONCAT", new List<object> { "Hello", "World!" } }
            };

            var results = testData.Select(data =>
                SpreadSheetFunctions.EvaluateFunction(data.Key, data.Value));

            results.ElementAt(0).Should().BeEquivalentTo(1);
            results.ElementAt(1).Should().BeEquivalentTo(3);
            results.ElementAt(2).Should().BeEquivalentTo(2);
            results.ElementAt(3).Should().BeEquivalentTo(2.0);
            results.ElementAt(4).Should().BeEquivalentTo(true);
            results.ElementAt(5).Should().BeEquivalentTo(true);
            results.ElementAt(6).Should().BeEquivalentTo(true);
            results.ElementAt(7).Should().BeEquivalentTo(false);
            results.ElementAt(8).Should().BeEquivalentTo(true);
            results.ElementAt(9).Should().BeEquivalentTo("Yes");
            results.ElementAt(10).Should().BeEquivalentTo("HelloWorld!");
        }
    }
}