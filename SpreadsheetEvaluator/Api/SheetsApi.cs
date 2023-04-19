using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl;
using Newtonsoft.Json;

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
            var sheets = await _client
                .Request($"/{endPoint}")
                .GetJsonAsync<Spreadsheet>();

            return sheets;
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
                // It took me HOURS to come up with this solution..
                Console.WriteLine($"An error occurred while making the request: {ex.Message}");
                Console.WriteLine($"Status code: {ex.Call.Response.StatusCode}");
                Console.WriteLine($"Response content: {await ex.GetResponseStringAsync()}");
                throw;
            }
        }
    }
}
