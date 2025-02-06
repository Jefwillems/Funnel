using System.CommandLine;
using System.CommandLine.Invocation;
using Funnel.CliClient;

var serverUrlArg = new Argument<Uri>("server_url",
    "The server URL to connect to");
var localUrlArg = new Argument<Uri>("local_url",
    "The local url to funnel requests to");
var command = new RootCommand("Funnel CLI client")
{
    serverUrlArg,
    localUrlArg
};

command.SetHandler(RunApp,
    serverUrlArg,
    localUrlArg);

await command.InvokeAsync(args);
return;

void RunApp(Uri serverUrl, Uri localUrl)
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddHttpClient()
        .ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            });
        });
    builder.Services.AddHostedService<Worker>(provider =>
        new Worker(provider.GetRequiredService<ILogger<Worker>>(),
            serverUrl,
            localUrl,
            "SampleConsumerId",
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<IHostApplicationLifetime>()));

    var host = builder.Build();
    host.Run();
}