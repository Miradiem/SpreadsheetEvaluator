using FluentAssertions;
using SpreadsheetEvaluator.Api;
using Xunit;

namespace SpreadsheetEvaluator.Tests.Api
{
    public class ApiClientTests
    {
        [Fact]
        public void ShouldCreateApiClient()
        {
            var sut = CreateSut();
            var result = sut.Create();

            result.BaseUrl.Should().Be("https://testingcall.com");
        }

        private ApiClient CreateSut() =>
            new ApiClient("https://testingcall.com");
    }
}