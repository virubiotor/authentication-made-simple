# Authentication made simple
Authentication is one of the foundational pillars of modern software development. It acts as the digital gatekeeper, ensuring that only the right users have access to the right resources. From safeguarding personal data to enforcing business logic, a secure authentication process is essential for maintaining user trust and system integrity.

However, despite its critical role, authentication is often seen as a cumbersome and intimidating part of the development process. Many developers find themselves wrestling with complex protocols, managing secrets, and navigating a labyrinth of security best practices just to get their applications off the ground. This can lead to a fragmented, error-prone approach that drains valuable development time and leaves critical gaps in security.

This repo aims to provide an opinionated roadmap to help improve knowledge about authentication and also provide some best practices for its implementation, encouraging a more secure and robust approach.

## Sample Scenarios
The samples folder contains a range of authentication scenarios based on the OpenID Connect (OIDC) protocol. Authentication is managed by a Duende Identity Server instance, with each example configured as a distinct client. These include:
- Machine-to-Machine Authentication: Using the Client Credentials flow.
- Interactive Authentication: Leveraging the Authorization Code flow with PKCE.
- Backend-for-Frontend (BFF) pattern: Implemented via the Authorization Code flow with PKCE.
- DPoP Authentication: Combining DPoP, BFF, and the Authorization Code flow.

## Additional Resources
While these samples provide a straightforward introduction to authentication, a deeper understanding of the underlying principles and technologies is essential for production use. Key areas to explore further include:
- OpenID Connect (OIDC) protocol
- Identity provider (IdP) configuration and management
- .NET implementation details
- Other authentication flows, such as token delegation

Below, you'll find a curated list of resources to help you expand your understanding and take the next steps in mastering authentication:

TBD
- API Resources
- API Scopes 
- Roles?
- Refresh tokens
- Token delegation
- Reference tokens
- What should you take into account to choose an IdP
- Security practices - https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics
- Best practices checklist
  - small tokens
  - claims contains no personal data
  
