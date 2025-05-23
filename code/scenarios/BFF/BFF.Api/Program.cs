// Copyright (c) Duende Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthentication("token")
    .AddJwtBearer("token", options =>
    {
        options.Authority = builder.Configuration.GetValue<string>("authority");
        options.MapInboundClaims = false;
        
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false,
            ValidTypes = ["at+jwt"],

            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiCaller", policy =>
    {
        policy.RequireClaim("scope", "authcode-bff");
    });

    options.AddPolicy("RequireInteractiveUser", policy =>
    {
        policy.RequireClaim("sub");
    });
});

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization("ApiCaller");

app.Run();
