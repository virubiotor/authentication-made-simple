```mermaid
sequenceDiagram
    participant User
    participant App
    participant BFF
    participant IdentityServer
    participant API

    User->>App: Click on Login
    App->>BFF: Call /login
    BFF->>IdentityServer: Call /authorize w/ challenge + secret
    IdentityServer-->>User: Redirect to auth
    User->>IdentityServer: Authenticate
    IdentityServer-->>BFF: Redirect w/ auth code
    loop Validate code
        BFF->>BFF: Validate code
    end
    BFF->>IdentityServer: Call /token w/ code_verifier
    loop Validations
        IdentityServer->>IdentityServer: Validate verifier & challenge
    end
    IdentityServer-->>BFF: Return token
    loop Issue Cookie
        BFF->>BFF: Issue Cookie
    end
    BFF-->>App: Callback with cookie
    App->>BFF: Call /orders endpoint w/ cookie
    loop Extract token
        BFF->>BFF: Retrieve token from cookie
    end
    BFF->>API: Call API w/ token
```