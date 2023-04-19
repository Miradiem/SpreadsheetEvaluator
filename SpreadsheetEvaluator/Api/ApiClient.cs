using Flurl;
using Flurl.Http;

namespace SpreadsheetEvaluator.Api
{
    public class ApiClient
    {
        private readonly string _baseUrl;

        public ApiClient(string baseUrl) =>
            _baseUrl = baseUrl;
        
        public IFlurlClient Create()
        {
            var client = new FlurlClient(_baseUrl);

            client.Settings.BeforeCall = (call) =>
            {
                Console.WriteLine($"Calling {call.HttpRequestMessage.RequestUri} <------------");
            };
            client.Settings.AfterCall = (call) =>
            {
                Console.WriteLine($"Call status code: {call.Response.StatusCode}");
            };
            client.Settings.OnError = (call) =>
            {
                Console.WriteLine($"{call.HttpResponseMessage.ReasonPhrase} --------->");
            };

            return client;
        }
            
    }
}