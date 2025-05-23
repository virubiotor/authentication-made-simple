using Duende.IdentityServer.Models;
using IdentityServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Idsrdb")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.AddServiceDefaults();

builder.Services.AddIdentityServer()
    .AddInMemoryClients([
        new Client
        {
            ClientId = "clientCredentials",
            ClientSecrets = [new Secret("secret".Sha256())],
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            AllowedScopes = { "app-client-credentials" }
        },
        new Client
        {
            ClientId = "authCode",
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris = ["http://localhost:5173/"],
            PostLogoutRedirectUris = { "http://localhost:5173/" },
            FrontChannelLogoutUri = "http://localhost:5173/",
            AllowedScopes = { "openid", "profile", "email", "phone", "app-code" },
            RequirePkce = true,
            RequireClientSecret = false
        },
        new Client
        {
            ClientId = "authCodeDPop",
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris = ["https://localhost:5002/signin-oidc"],
            AllowedScopes = { "openid", "profile", "email", "authcode-dpop", "offline_access" },
            AllowOfflineAccess = true,
            RequirePkce = true,
            ClientSecrets = [new Secret("dpopsecret".Sha256())],
        },
        new Client
        {
            ClientId = "authCodeBFF",
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris = ["https://localhost:5010/signin-oidc"],
            AllowedScopes = { "openid", "profile", "email", "authcode-bff", "offline_access" },
            AllowOfflineAccess = true,
            RequirePkce = true,
            ClientSecrets = [new Secret("BFFSecret".Sha256())],
        }
    ])
    .AddInMemoryIdentityResources([
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email(),
        new IdentityResources.Phone(),
    ])
    .AddInMemoryApiResources([
        new ApiResource{
            Name= "app-code",
            Scopes = ["app-code"]
        },
        new ApiResource{
            Name="app-client-credentials",
            Scopes = ["app-client-credentials"]
        },
        new ApiResource{
            Name="authcode-dpop",
            Scopes = ["authcode-dpop"]
        },
        new ApiResource{
            Name="authcode-bff",
            Scopes = ["authcode-bff"]
        }
        ])
    .AddInMemoryApiScopes([
        new ApiScope("app-code"),
        new ApiScope("app-client-credentials"),
        new ApiScope("authcode-dpop"),
        new ApiScope("authcode-bff")
        ])
    .AddAspNetIdentity<IdentityUser>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:5173");
        });
});

builder.Services.AddLogging(options =>
{
    options.AddFilter("Duende", LogLevel.Debug);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.UseIdentityServer();
app.UseAuthorization();

app.MapRazorPages();

app.Run();