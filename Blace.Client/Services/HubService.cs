using Blace.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

namespace Blace.Client.Services;

public class HubService
{
    private readonly IServiceProvider _serviceProvider;
    
    public HubService(IWebAssemblyHostEnvironment env, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Connection = new HubConnectionBuilder()
            .WithUrl(
                GetServerAddress(env) + "Game",
                o => o.AccessTokenProvider = () => Task.FromResult(UserId.ToString())!)
            .AddMessagePackProtocol()
            .WithAutomaticReconnect()
            .Build();
        Server = Connection.GetServerProxy<IServer>();
    }

    private HubConnection Connection { get; }
    public IServer Server { get; }
    public int UserId { get; private set; }

    public async Task Start()
    {
        foreach (IClient client in _serviceProvider.GetRequiredService<IEnumerable<IClient>>())
            RegisterClient(client);

        await Connection.StartAsync();
    }

#pragma warning disable IDE0001
    // ReSharper disable once RedundantTypeArgumentsOfMethod
    public IDisposable RegisterClient(IClient client) => Connection.RegisterClient<IClient>(client);
#pragma warning restore IDE0001

    private static string GetServerAddress(IWebAssemblyHostEnvironment env) =>
        !env.IsDevelopment()
            ? "https://blace-server.azurewebsites.net/"
            : env.BaseAddress.Contains("7150")
                ? env.BaseAddress.Replace("7150", "7151")
                : throw new($"Couldn't resolve address of server for base address {env.BaseAddress}");
}

[AttributeUsage(AttributeTargets.Method)]
internal class HubServerProxyAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
internal class HubClientProxyAttribute : Attribute
{
}

internal static partial class MyCustomExtensions
{
    [HubClientProxy]
    public static partial IDisposable RegisterClient<T>(this HubConnection connection, T provider);

    [HubServerProxy]
    public static partial T GetServerProxy<T>(this HubConnection connection);
}
