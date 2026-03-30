using Blace.Server;
using Blace.Server.Data;
using Blace.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Sentry.AspNetCore;
using Constants = Blace.Server.Constants;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddCors();

var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres");
if (postgresConnectionString == null)
    throw new("ConnectionStrings:Postgres not set.");
builder.Services.AddPooledDbContextFactory<Db>(db =>
{
    db.UseNpgsql(postgresConnectionString);

    if (builder.Environment.IsDevelopment())
        db.EnableSensitiveDataLogging();
});

builder.Services.AddSingleton<IPlaceRepository, EfPlaceRepository>();

builder.Services.AddSingleton<PlaceService>();
builder.Services.AddSingleton<PlayerService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
builder.Services
    .AddSignalR(o => o.MaximumReceiveMessageSize = null)
    .AddMessagePackProtocol();

if (builder.Configuration["Sentry:Dsn"] != null)
{
    builder.WebHost.UseSentry(
        (Action<SentryAspNetCoreOptions>)builder.Configuration.GetSection("Sentry").Bind);
}

// TODO: AUTH
builder.Services.AddAuthorization(o => o.AddPolicy(Constants.AdminPolicy, p => p.RequireAssertion(_ => true)));

WebApplication app = builder.Build();

{
    var repository = app.Services.GetService<IPlaceRepository>();
    if (repository is EfPlaceRepository efPlaceRepository)
        await efPlaceRepository.Initialize();
}
await app.Services.GetRequiredService<PlaceService>().Initialize();

app.UseCors(cors => cors
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

if (!app.Environment.IsDevelopment())
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapHub<Server>("/Game");

app.Run();