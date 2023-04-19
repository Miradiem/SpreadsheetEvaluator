using Flurl;
using Flurl.Http;

namespace SpreadsheetEvaluator.Api
{
    public class ApiClient
    {
        private readonly string _baseUrl;

        public ApiClient(string baseUrl) =>
            _baseUrl = baseUrl;
        
        public IFlurlClient Create() =>
            new FlurlClient(_baseUrl);    
    }
}