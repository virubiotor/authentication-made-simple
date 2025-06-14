// Copyright (c) Duende Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace Api;

[Route("identity")]
public class IdentityController(ILogger<IdentityController> logger) : ControllerBase
{
    private readonly ILogger<IdentityController> _logger = logger;

    // this action simply echoes the claims back to the client
    [HttpGet]
    public ActionResult Get()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        var accessToken = authHeader[7..];
        var accessTokenUrl = $"https://jwt.ms/#access_token={accessToken}";
        _logger.LogInformation("View the decoded access_token! {accessTokenUrl}", accessTokenUrl);
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        _logger.LogInformation("claims: {claims}", claims);

        return new JsonResult(claims);
    }
}
