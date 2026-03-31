using Blazored.LocalStorage;
using Blace.Client;
using Blace.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

try
{
    WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

    if (builder.Configuration["Sentry:Dsn"] != null)
    {
        SentryOptions sentryOptions = new() { Environment = builder.HostEnvironment.Environment.ToLower() };
        builder.Configuration.GetSection("Sentry").Bind(sentryOptions);
        SentrySdk.Init(sentryOptions);
    }

    builder.Services.AddAuthorizationCore();
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddAuthenticationStateDeserialization();
    builder.Services.AddSingleton<HubService>();
    builder.Services.AddSingleton(s => s.GetRequiredService<HubService>().Server);
    builder.Services.AddSingleton<EventService>();
    builder.Services.AddHubClientSingleton<CooldownService>();
    builder.Services.AddHubClientSingleton<PlaceService>();
    builder.Services.AddBlazoredLocalStorageAsSingleton();

    builder.Logging.AddSentry(o => o.InitializeSdk = false);

    WebAssemblyHost app = builder.Build();

    await app.Services.GetRequiredService<HubService>().Start();

    await Task.WhenAll(
        app.Services.GetRequiredService<PlaceService>().Initialize(),
        app.Services.GetRequiredService<CooldownService>().Initialize()
    );

    await app.RunAsync();
}
catch (Exception e)
{
    if (SentrySdk.IsEnabled)
    {
        SentrySdk.CaptureException(e);
        await SentrySdk.FlushAsync(TimeSpan.FromSeconds(2));
    }
    throw;
}