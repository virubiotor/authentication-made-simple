// Copyright (c) Duende Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Duende.Bff.Yarp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddBff();

var yarpBuilder = builder.Services.AddReverseProxy()
    .AddTransforms<AccessTokenTransformProvider>();
//Configure from included extension method
yarpBuilder.Configure();

// registers HTTP client that uses the managed user access token
builder.Services.AddUserAccessTokenHttpClient("api_client", configureClient: client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("apiUrl")!);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookie";
    options.DefaultChallengeScheme = "oidc";
    options.DefaultSignOutScheme = "oidc";
})
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "__Host-bff";
        options.Cookie.SameSite = SameSiteMode.Strict;
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = builder.Configuration.GetValue<string>("authority");
        
        // confidential client using code flow + PKCE
        options.ClientId = "authCodeBFF";
        options.ClientSecret = "BFFSecret";
        options.ResponseType = "code";
        options.ResponseMode = "query";

        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false;
        options.SaveTokens = true;
        options.DisableTelemetry = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("authcode-bff");
        options.Scope.Add("offline_access");
        options.Scope.Add("email");
    });


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseBff();
app.UseAuthorization();

app.MapBffManagementEndpoints();

// if you want the TODOs API local
// endpoints.MapControllers()
//     .RequireAuthorization()
//     .AsBffApiEndpoint();

// if you want the TODOs API remote
app.MapBffReverseProxy();

// which is equivalent to
//endpoints.MapReverseProxy()
//    .AsBffApiEndpoint();

app.Run();
