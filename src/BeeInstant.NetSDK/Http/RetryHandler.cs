using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BeeInstant.NetSDK.Http
{
    public class RetryHandler : DelegatingHandler
    {
        private readonly int maxRetries;

        public RetryHandler(HttpMessageHandler innerHandler, int retries = 3) : base(innerHandler)
        { 
            maxRetries = retries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < maxRetries; i++)
            {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
            }

            return response;
        }
    }
}