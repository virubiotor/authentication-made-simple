```mermaid
sequenceDiagram
    participant App
    participant IdentityServer
    participant Api

    App->>IdentityServer: Call /token w/ client secret
    loop Validate secret
        IdentityServer->>IdentityServer: Validate secret
    end
    IdentityServer-->>App: Return token
    App->>Api: Call w/ Access Token
```