using System.Security.Cryptography.X509Certificates;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityServer;
using IdentityServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("IdentityServerDatabase")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // WARNING: Not recommended to do this!! Only for demo purposes!
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.AllowedForNewUsers = false;
}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddAuthentication()
    .AddCertificate(op =>
    {
        // Revocation check disabled for mkcert certificate. 
        // In production, revocation should be checked.

        op.RevocationMode = X509RevocationMode.NoCheck;
    }
);
builder.AddServiceDefaults();

// Warning: This is not recommended for production use! Only for demo purposes!
X509Certificate2 ClientCert() => new("../../localhost-client.p12", "changeit");
string ClientCertificateThumbprint() => ClientCert().Thumbprint;
string ClientCertificateSubject() => ClientCert().Subject;
builder.Services.AddIdentityServer(op =>
    {
        op.MutualTls.Enabled = true;
    })
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
        },
        new Client
        {
            ClientId = "mtls",
            AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
            RedirectUris = ["https://localhost:5011/signin-oidc"],
            AllowedScopes = { "openid", "profile", "scope1" },
            AllowOfflineAccess = true,
            ClientSecrets =
            {
                //(Either secret type can be used)
                new Secret(ClientCertificateThumbprint())
                {
                    Type = IdentityServerConstants.SecretTypes.X509CertificateThumbprint
                },
                // new Secret(ClientCertificateSubject())
                // {
                //     Type = IdentityServerConstants.SecretTypes.X509CertificateName
                // }
            }
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
        },
        new ApiResource("scope1")
        {
            Scopes = { "scope1" },
            UserClaims = { "scope1" }
        }
        ])
    .AddInMemoryApiScopes([
        new ApiScope("app-code"),
        new ApiScope("app-client-credentials"),
        new ApiScope("authcode-dpop"),
        new ApiScope("authcode-bff"),
        new ApiScope("scope1")
        ])
    .AddAspNetIdentity<IdentityUser>()
    .AddMutualTlsSecretValidators();

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

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(https =>
    {
        https.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
        https.AllowAnyClientCertificate();   // Demo purposes only!! trusts every client cert
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await SeedData.EnsureSeedData(scope.ServiceProvider);
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