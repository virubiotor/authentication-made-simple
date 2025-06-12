```mermaid
sequenceDiagram
    participant App
    participant IdentityServer
    participant Api

    App->>IdentityServer: Llamada /token con client secret
    loop Validación secreto
        IdentityServer->>IdentityServer: Validación secreto
    end
    IdentityServer-->>App: Devuelve token
    App->>Api: Llamada con Access Token
```