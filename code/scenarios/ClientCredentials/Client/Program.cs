using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

var client = new HttpClient();
var tokenUrl = "https://localhost:5001/connect/token";
var parameters = new Dictionary<string, string>
{
   { "address", tokenUrl },
   { "client_id", "clientCredentials" },
   { "client_secret", "secret" },
   { "grant_type", "client_credentials" },
   { "scope", "app-client-credentials" }
};


using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
request.Headers.Add("Accept", "application/json");
request.Content = new FormUrlEncodedContent(parameters);
var tokenResponse = await client.SendAsync(request);

string responseContent = await tokenResponse.Content.ReadAsStringAsync();

Console.WriteLine($"Token response: {responseContent}");

using JsonDocument doc = JsonDocument.Parse(responseContent);
string accessToken = doc.RootElement.GetProperty("access_token").GetString();

Console.WriteLine("Enter any key to call service");
Console.ReadLine();

var consumerClient = new HttpClient();
consumerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
using HttpResponseMessage response = await consumerClient.GetAsync("https://localhost:7046/weatherforecast");

response.EnsureSuccessStatusCode();

var jsonResponse = await response.Content.ReadAsStringAsync();
Console.WriteLine($"{jsonResponse}\n");


