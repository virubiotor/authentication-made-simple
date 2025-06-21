# Authentication made simple

[View this repo in DeepWiki](https://deepwiki.com/virubiotor/authentication-made-simple)

Authentication is one of the foundational pillars of modern software development. It acts as the digital gatekeeper, ensuring that only the right users have access to the right resources. From safeguarding personal data to enforcing business logic, a secure authentication process is essential for maintaining user trust and system integrity.

However, despite its critical role, authentication is often seen as a cumbersome and intimidating part of the development process. Many developers find themselves wrestling with complex protocols, managing secrets, and navigating a labyrinth of security best practices just to get their applications off the ground. This can lead to a fragmented, error-prone approach that drains valuable development time and leaves critical gaps in security.

This repo aims to provide an opinionated roadmap to help improve knowledge about authentication and also provide some best practices for its implementation, encouraging a more secure and robust approach.

## Sample Scenarios
The `scenarios` folder contains a range of authentication scenarios based on the OpenID Connect (OIDC) protocol. Authentication is managed by a Duende Identity Server instance, with each example configured as a distinct client. These include:
- **Machine-to-Machine Authentication:** Using the Client Credentials flow.
- **Interactive Authentication:** Leveraging the Authorization Code flow with PKCE.
- **Backend-for-Frontend (BFF) pattern:** Implemented via the Authorization Code flow with PKCE.
- **PoP with mTLS Authentication:** Proof of Possession (PoP) using mutual TLS (mTLS).
- **DPoP Authentication:** Combining DPoP, BFF, and the Authorization Code flow.

## Demo

To run the demo, follow these steps:
1. Clone the repository:
2. Open the solution in Visual Studio or your preferred IDE.
3. Start the Aspire dashboard project (`AuthenticationMadeSimple.AppHost`), which will run the Identity Server along with the sample projects.
4. Feel free to explore the various authentication scenarios in the `scenarios` folder.


**Note:** a default user for testing purposes is provided in the Identity Server configuration. You can use the following credentials to log in:
  - Username: `dotnet@example.org`
  - Password: `Test1!`

**Note:** the demo for PoP with mTLS Authentication requires a valid certificate. You can generate a self-signed certificate using the `mkcert` tool or any other method you prefer. With `mkcert`, you can run the following command to create and provide a certificate:
```bash
# First ensure mkcert is installed and configured
# Refer to https://github.com/FiloSottile/mkcert and follow the installation instructions for your platform.
mkcert -install

# Generate a self-signed certificate for localhost
mkcert -client -pkcs12 localhost
# This will create a file named localhost-client.p12 in the current directory.
# Certificates last for 2 years and 3 months by default...
```

## Additional Resources
While these samples provide a straightforward introduction to authentication, a deeper understanding of the underlying principles and technologies is essential for production use. Key areas to explore further include:
- OpenID Connect (OIDC) protocol
- Identity provider (IdP) configuration and management
- .NET implementation details
- Other authentication flows, such as token delegation

Below, you'll find a curated list of resources to help you expand your understanding and take the next steps in mastering authentication:
- Understanding fundamentals (regardless of IdPs) - base documentation from Duende IdentityServer ([Duende IdentityServer Docs](https://docs.duendesoftware.com))
  - Clients ([Duende IdentityServer Docs: Clients](https://docs.duendesoftware.com/identityserver/reference/models/client))
  - API Resources ([Duende IdentityServer Docs: API Resources](https://docs.duendesoftware.com/identityserver/fundamentals/resources/#api-resources))
  - API Scopes ([Duende IdentityServer Docs: API Scopes](https://docs.duendesoftware.com/identityserver/fundamentals/resources/#api-resources))
  - Claims ([Duende IdentityServer Docs: Claims](https://docs.duendesoftware.com/identityserver/fundamentals/claims/))
- Awesome playground for a deeper understanding of OAuh flows ([OAuth2 Playground](https://www.oauth.com/playground/))
  - Implicit flow is not recommended anymore, Auth code + PKCE should be used instead
  - Refresh tokens ([OAuth 2.0 Refresh Token](https://datatracker.ietf.org/doc/html/rfc6749#section-6))
  - Token delegation ([OAuth 2.0 Token Exchange](https://datatracker.ietf.org/doc/html/rfc8693))
  - Reference tokens ([Duende IdentityServer Docs: Reference Tokens](https://docs.duendesoftware.com/identityserver/tokens/reference))
- Choosing an IdP ([How to Choose an Identity Provider](https://securityblog.omegapoint.se/en/how-to-choose-an-idp))
- Dominick Baier POV on authorization + roles ([Identity vs permissions](https://leastprivilege.com/2016/12/16/identity-vs-permissions/))
- Roles ([Microsoft Docs: Role-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles))
- Best practices:
  - Improving authentication by using "form post" ([OIDC form post response](https://openid.net/specs/oauth-v2-form-post-response-mode-1_0.html))
  - Security practices ([OAuth 2.0 Security Best Current Practice](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics))
  - Best practices checklist ([OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html))
    - Use small tokens
    - Ensure claims contain no personal data
  - Same Site cookies ([Least privilege blog](https://leastprivilege.com/2019/01/18/an-alternative-way-to-secure-spas-with-asp-net-core-openid-connect-oauth-2-0-and-proxykit/))
