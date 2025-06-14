// Copyright (c) Duende Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using System.Text.Json;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;

namespace PoPmTLS.Client;

public static class TokenResponseExtensions
{
    public static void Show(this TokenResponse response, ILogger logger)
    {
        if (!response.IsError)
        {
            logger.LogInformation("Token response:\n" + response.Json);

            if (response.AccessToken.Contains("."))
            {
                logger.LogInformation("\nAccess Token (decoded):");

                response.AccessToken.ShowAccessToken(logger);
            }
        }
        else
        {
            if (response.ErrorType == ResponseErrorType.Http)
            {
                logger.LogError("HTTP error: " + response.Error);
                logger.LogError("HTTP status code: " + response.HttpStatusCode);
            }
            else
            {
                logger.LogError("Protocol error response:\n" + response.Raw);
            }
        }
    }
    
    private static void ShowAccessToken(this string accessToken, ILogger logger)
    {
        var parts = accessToken.Split('.');
        var header = parts[0];
        var payload = parts[1];

        logger.LogInformation(JsonSerializer.Serialize(JsonDocument.Parse(Encoding.UTF8.GetString(Base64Url.Decode(header))), new JsonSerializerOptions { WriteIndented = true }));
        logger.LogInformation(JsonSerializer.Serialize(JsonDocument.Parse(Encoding.UTF8.GetString(Base64Url.Decode(payload))), new JsonSerializerOptions { WriteIndented = true }));
    }
}
