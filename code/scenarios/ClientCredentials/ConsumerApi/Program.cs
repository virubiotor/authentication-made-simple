using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication()
    .AddJwtBearer(jwtOptions =>
    {
        var authority = builder.Configuration.GetValue<string>("authority");
        jwtOptions.Authority = builder.Configuration.GetValue<string>("authority");
        jwtOptions.Audience = "app-client-credentials";

        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authority,
        };

        jwtOptions.SaveToken = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", [Authorize] (HttpContext context) =>
{
    Console.WriteLine("Call to weather forecast endpoint with token {0}", context.Request.Headers.Authorization.ToString());
    var authHeader = context.Request.Headers.Authorization.ToString();
    var accessToken = authHeader[7..];
    var accessTokenUrl = $"https://jwt.ms/#access_token={accessToken}";
    app.Logger.LogInformation("View the decoded access_token! {accessTokenUrl}", accessTokenUrl);

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
