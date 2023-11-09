using System;
using System.Net;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Polly;
using Polly.Retry;
namespace FuncQueue
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public Function1(ILogger<Function1> logger,IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="message"></param>
        [Function(nameof(Function1))]
        public async Task RunAsync([QueueTrigger("myqueue-items", Connection = "ConnectionString1234")] 
            QueueMessage message , FunctionContext ctx)
        {
            
            try
            {
                var zip = message.MessageText;
               _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText} ");

                var httpClient = _httpClientFactory.CreateClient("ZipCode");
                var response = await httpClient.GetAsync($"{zip}");

                if (response.StatusCode != HttpStatusCode.OK) throw new Exception();

            
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Retry count: {ctx.RetryContext?.RetryCount}  at {DateTime.Now}");
                throw;
            }
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
        }
    }
}
