// Copyright (c) Duende Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using Duende.IdentityModel.Client;

namespace PoPmTLS.Client;

public class Worker : BackgroundService
{
    
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("PoPmTLS.Client running at: {time}", DateTimeOffset.Now);
            }

            try
            {
                var response = await RequestTokenAsync();
                response.Show(_logger);

                await CallServiceAsync(response.AccessToken!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the PoPmTLS.Client.");
            }

            await Task.Delay(20000, stoppingToken); // Delay for 20 seconds before the next iteration to request a new token
        }
    }

    private async Task<TokenResponse> RequestTokenAsync()
    {
        var client = _httpClientFactory.CreateClient("idpClient");

        var disco = await client.GetDiscoveryDocumentAsync();
        if (disco.IsError)
        {
            throw new Exception(disco.Error);
        }

        var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.MtlsEndpointAliases!.TokenEndpoint,

            ClientId = "mtls",
            ClientCredentialStyle = ClientCredentialStyle.PostBody,
            Scope = "scope1"
        });

        if (response.IsError)
        {
            throw new Exception(response.Error);
        }

        return response;
    }

    private async Task CallServiceAsync(string token)
    {
        var client = _httpClientFactory.CreateClient("apiClient");
        client.SetBearerToken(token);
        var accessTokenUrl = $"https://jwt.ms/#access_token={token}";
        Console.WriteLine($"Calling service with mTLS and access token: {accessTokenUrl}");
        
        var response = await client.GetStringAsync("/identity");
        Console.WriteLine("\n\nService claims:\n" + JsonSerializer.Serialize(JsonDocument.Parse(response), new JsonSerializerOptions { WriteIndented = true }));
    }
}
