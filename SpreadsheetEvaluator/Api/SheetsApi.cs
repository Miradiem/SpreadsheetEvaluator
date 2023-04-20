using Flurl.Http;

namespace SpreadsheetEvaluator.Api
{
    public class SheetsApi
    {
        private readonly IFlurlClient _client;

        public SheetsApi(IFlurlClient client)
        {
            _client = client;
        }

        public async Task<Spreadsheet> GetSheets(string endPoint)
        {
            try
            {
                var sheets = await _client
               .Request($"/{endPoint}")
               .GetJsonAsync<Spreadsheet>();

                return sheets;
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine($"An error occurred while making the request: {ex.Message}");
                Console.WriteLine($"Status code: {ex.Call.Response.StatusCode}");
                Console.WriteLine($"Response content: {await ex.GetResponseStringAsync()}");
                throw;
            }
        }

        public async Task<string> PostSubmissions(string endPoint, SubmissionResult submissionResult)
        {
            try
            {
                var response = await _client
                    .Request($"{endPoint}")
                    .PostJsonAsync(submissionResult)
                    .ReceiveJson<ResponseMessage>();

                return response.Message;
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine($"An error occurred while making the request: {ex.Message}");
                Console.WriteLine($"Status code: {ex.Call.Response.StatusCode}");
                Console.WriteLine($"Response content: {await ex.GetResponseStringAsync()}");
                throw;
            }
        }
    }
}
