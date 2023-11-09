using Microsoft.Extensions.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;
using Polly.Extensions.Http;


var retryPolicyHandler = HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
    .WaitAndRetryAsync(retryCount: 4,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));


var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(2)
    );

var builder = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults();

builder.ConfigureServices((ctx, services) =>
    {
        services.AddHttpClient("ZipCode", client =>
            {
                client.BaseAddress = new Uri("https://api.zippopotam.us/us/");
            })
            .AddPolicyHandler(retryPolicyHandler)
            .AddPolicyHandler(circuitBreakerPolicy);
    }
   
);
 

builder.Build().Run();