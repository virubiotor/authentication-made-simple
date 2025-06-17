```mermaid
sequenceDiagram
    participant Client
    participant IdentityServer
    participant API

    Client->>IdentityServer: Petición Token + "prueba" DPoP
    IdentityServer-->>Client: AT vinculado por DPoP con token_type=DPoP
    Client->>API: Petición + AT + "prueba" DPoP 
    loop Validaciones
        API->>API: Validación
    end
```