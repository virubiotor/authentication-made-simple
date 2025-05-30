using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("consumer-api", o =>
{
    o.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ConsumerApiUrl")!);
});

builder.Services.AddHttpClient("auth", o =>
{
    o.BaseAddress = new Uri(builder.Configuration.GetValue<string>("authority")!);
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", async ([FromServices] IHttpClientFactory httpClientFactory) =>
{
    var authClient = httpClientFactory.CreateClient("auth");
    var parameters = new Dictionary<string, string>
    {
       { "address", $"{authClient.BaseAddress}/connect/token" },
       { "client_id", "clientCredentials" },
       { "client_secret", "secret" },
       { "grant_type", "client_credentials" },
       { "scope", "app-client-credentials" }
    };


    app.Logger.LogInformation("Requesting token: POST https://localhost:5001/connect/token. Parameters: {RequestParameters}", parameters);
    using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("connect/token",UriKind.Relative));
    request.Headers.Add("Accept", "application/json");
    request.Content = new FormUrlEncodedContent(parameters);
    var tokenResponse = await authClient.SendAsync(request);

    string responseContent = await tokenResponse.Content.ReadAsStringAsync();

    app.Logger.LogInformation("Response received!. Token response: {responseContent}", responseContent);

    using JsonDocument doc = JsonDocument.Parse(responseContent);
    string accessToken = doc.RootElement.GetProperty("access_token").GetString()!;
    
    app.Logger.LogInformation("Access token: {accessToken}", accessToken);
    var accessTokenUrl = $"https://jwt.ms/#access_token={accessToken}";
    app.Logger.LogInformation("View the decoded access_token! {accessTokenUrl}", accessTokenUrl);

    app.Logger.LogInformation("Requesting data from API with token: GET /weatherforecast");
    var client = httpClientFactory.CreateClient("consumer-api");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync("weatherforecast");
    var jsonResponse = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
    app.Logger.LogInformation("Response received from API!");
    return jsonResponse;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureF, string? Summary)
{
}
