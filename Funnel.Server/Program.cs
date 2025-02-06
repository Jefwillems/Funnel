using System.Text;
using System.Threading.Channels;
using Funnel.Server;
using Funnel.Server.Services;
using Google.Protobuf.Collections;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionService, ConnectionService>();

// Add services to the container.
builder.Services.AddGrpc();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<FunnelService>();

if (builder.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Map("{consumerName}/{*path}",
    async (string consumerName, string path, HttpContext ctx, CancellationToken cancellationToken) =>
    {
        var channelService = ctx.RequestServices.GetRequiredService<IConnectionService>();
        var x = new FunneledRequest()
        {
            Id = Guid.NewGuid().ToString("N"),
            Body = await GetRequestBodyAsync(ctx),
            Method = ctx.Request.Method,
            Path = path,
        };
        foreach (var (key, value) in ctx.Request.Headers)
        {
            x.Headers.Add(key, value);
        }

        foreach (var (key, value) in ctx.Request.Query)
        {
            x.Query.Add(key, value);
        }

        var response = await channelService.SendAndWaitForResponse(consumerName, x);
        ctx.Response.StatusCode = response.Status;
        await ctx.Response.WriteAsync(response.Body, cancellationToken: cancellationToken);
    });

app.Run();
return;


static async Task<string> GetRequestBodyAsync(HttpContext context)
{
    context.Request.EnableBuffering();
    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
    var body = await reader.ReadToEndAsync();
    context.Request.Body.Position = 0;
    return body;
}