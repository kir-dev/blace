using Blace.Server;
using Blace.Server.Data;
using Blace.Server.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Sentry.AspNetCore;
using Constants = Blace.Server.Constants;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres");
if (postgresConnectionString == null)
    throw new("ConnectionStrings:Postgres not set.");
builder.Services.AddPooledDbContextFactory<Db>(db =>
{
    db.UseNpgsql(postgresConnectionString);

    if (builder.Environment.IsDevelopment())
        db.EnableSensitiveDataLogging();
});

builder.Services.AddDataProtection().PersistKeysToDbContext<Db>();

builder.Services.AddSingleton<IPlaceRepository, EfPlaceRepository>();

builder.Services.AddScoped<UserInfoService>();
builder.Services.AddSingleton<PlaceService>();
builder.Services.AddSingleton<PlayerCountService>();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
builder.Services
    .AddSignalR(o => o.MaximumReceiveMessageSize = null)
    .AddMessagePackProtocol();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

if (builder.Configuration["Sentry:Dsn"] != null)
{
    builder.WebHost.UseSentry(
        (Action<SentryAspNetCoreOptions>)builder.Configuration.GetSection("Sentry").Bind);
}

// Authentication
builder.Services.AddAuthentication(options =>
    {
        // Sign in using AuthSCH
        options.DefaultChallengeScheme = Constants.AuthSchAuthenticationScheme;

        // Store the user's identity in a cookie
        options.DefaultScheme = Constants.CookieAuthenticationScheme;
    })
    .AddCookie(Constants.CookieAuthenticationScheme, options => options.Cookie.Name = "User")
    .AddOpenIdConnect(Constants.AuthSchAuthenticationScheme, options =>
    {
        options.Authority = "https://auth.sch.bme.hu";

        options.ClientId = builder.Configuration["AuthSch:ClientId"];
        options.ClientSecret = builder.Configuration["AuthSch:ClientSecret"];

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("offline_access");
        options.Scope.Add("pek.sch.bme.hu:profile");
        options.Scope.Add("email");
        // To retrieve a claim only available through the AuthSCH user info endpoint
        // (https://git.sch.bme.hu/kszk/authsch/-/wikis/api#a-userinfo-endpoint),
        // add its corresponding scope here, then map the claim in UserInfoService.

        options.ResponseType = "code";
        options.ResponseMode = "query";
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "roles";

        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false; // Disable messing with claim names
    });
builder.Services.AddCascadingAuthenticationState();

// After the user logs in, we receive an Authorization Code from AuthSCH, which is then automatically redeemed
// by ASP.NET for an access token, an ID token and a refresh token.
// These are then stored in the user's cookie (`options.SaveTokens = true`).
//
// As the ID token does not contain things like group memberships, the AuthSCH UserInfo endpoint is queried using the
// access token (`options.GetClaimsFromUserInfoEndpoint = true`).
// The UserInfo endpoint returns JSON data, as documented on the AuthSCH wiki.
//
// In the below code, we hook into the UserInformationReceived event to set the "memberships" claim for the user.
string publicUrl = builder.Configuration["StartSch:PublicUrl"]!;
string userAgent = $"StartSCHBot/1.0 (+{publicUrl})";
builder.Services.AddOptions<OpenIdConnectOptions>(Constants.AuthSchAuthenticationScheme)
    .PostConfigure(((OpenIdConnectOptions options, IServiceProvider serviceProvider) =>
    {
        options.Events.OnUserInformationReceived = async context =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            await scope.ServiceProvider
                .GetRequiredService<UserInfoService>()
                .OnUserInformationReceived(context);
        };

        options.Backchannel.DefaultRequestHeaders.Add(HeaderNames.UserAgent, userAgent);
    }));

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(Constants.AdminPolicy, p => p.RequireRole("admin"));

builder.Services.AddControllers();

// Set the requester's IP address and the original protocol using headers set by the reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Clear(); // trust headers from all proxies
    options.KnownIPNetworks.Clear();
});

WebApplication app = builder.Build();

{
    var repository = app.Services.GetService<IPlaceRepository>();
    if (repository is EfPlaceRepository efPlaceRepository)
        await efPlaceRepository.Initialize();
}
await app.Services.GetRequiredService<PlaceService>().Initialize();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(o => o.ContentSecurityFrameAncestorsPolicy = "'self' https://client.indulasch.kir-dev.hu")
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Blace.Client._Imports).Assembly);
app.MapBlazorHub();
app.MapHub<Server>("/Game");

await app.RunAsync();