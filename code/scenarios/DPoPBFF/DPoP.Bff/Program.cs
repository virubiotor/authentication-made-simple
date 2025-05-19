// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.Yarp;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Security.Cryptography;
using System.Text.Json;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
    .MinimumLevel.Override("IdentityModel", LogEventLevel.Debug)
    .MinimumLevel.Override("Duende.Bff", LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
var yarpBuilder = builder.Services.AddReverseProxy()
                .AddBffExtensions();
yarpBuilder.Configure();

builder.Services.AddSerilog();
// Add BFF services to DI - also add server-side session management
builder.Services.AddBff(options =>
{
    var rsaKey = new RsaSecurityKey(RSA.Create(2048));
    var jwkKey = JsonWebKeyConverter.ConvertFromSecurityKey(rsaKey);
    jwkKey.Alg = "PS256";
    var jwk = JsonSerializer.Serialize(jwkKey);
    options.DPoPJsonWebKey = jwk;
})
.AddRemoteApis()
.AddServerSideSessions();

// local APIs
builder.Services.AddControllers();

// cookie options
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookie";
    options.DefaultChallengeScheme = "oidc";
    options.DefaultSignOutScheme = "oidc";
})
    .AddCookie("cookie", options =>
    {
        // set session lifetime
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        // sliding or absolute
        options.SlidingExpiration = false;

        // host prefixed cookie name
        options.Cookie.Name = "__Host-bff-dpop";

        // strict SameSite handling
        options.Cookie.SameSite = SameSiteMode.Strict;
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = builder.Configuration.GetValue<string>("authority");

        // confidential client using code flow + PKCE
        options.ClientId = "authCodeDPop";
        options.ClientSecret = "dpopsecret";
        options.ResponseType = "code";
        options.ResponseMode = "query";

        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
        options.DisableTelemetry = true;

        // request scopes + refresh tokens
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("authcode-dpop");
        options.Scope.Add("offline_access");
        options.Scope.Add("email");
    });

builder.Services.AddUserAccessTokenHttpClient("api",
    configureClient: client =>
    {
        client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("apiUrl")!);
    });

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseDeveloperExceptionPage();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseRouting();

// adds antiforgery protection for local APIs
app.UseBff();

// adds authorization for local and remote API endpoints
app.UseAuthorization();

app.MapControllers()
        .RequireAuthorization()
        .AsBffApiEndpoint();

// login, logout, user, backchannel logout...
app.MapBffManagementEndpoints();

// proxy endpoints using yarp
app.MapReverseProxy(proxyApp =>
{
    proxyApp.UseAntiforgeryCheck();
});

app.MapRemoteUrls();

app.Run();