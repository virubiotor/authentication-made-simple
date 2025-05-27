using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("SQLServer", port: 1433)
                 .WithLifetime(ContainerLifetime.Persistent)
                 .AddDatabase("IdentityServerDatabase");

var idsr = builder.AddProject<Projects.IdentityServer>("IdentityServer")
       .WithReference(sql)
       .WaitFor(sql)
       .WithExternalHttpEndpoints();

var consumerApi = builder.AddProject<Projects.ConsumerApi>("ClientCredentials-ConsumerApi")
       .WithReference(idsr)
       .WaitFor(idsr)
       .WithExternalHttpEndpoints();

builder.AddProject<Projects.ClientApi>("ClientCredentials-Client")
       .WithReference(idsr)
       .WithReference(consumerApi)
       .WaitFor(consumerApi)
       .WithExternalHttpEndpoints();

var dpopApi = builder.AddProject<Projects.DPoP_Api>("DPop-Api")
       .WaitFor(idsr)
       .WithReference(idsr)
       .WithExternalHttpEndpoints();

builder.AddProject<Projects.DPoP_Bff>("DPop-Bff")
       .WithReference(idsr)
       .WithReference(dpopApi)
       .WaitFor(dpopApi)
       .WithExternalHttpEndpoints();

var bffApi = builder.AddProject<Projects.BFF_Api>("BFF-Api")
       .WaitFor(idsr)
       .WithReference(idsr)
       .WithExternalHttpEndpoints();

builder.AddProject<Projects.BFF>("BFF")
       .WithReference(idsr)
       .WithReference(bffApi)
       .WaitFor(bffApi)
       .WithExternalHttpEndpoints();

builder.AddNpmApp("AuthenticationCode-Client", "../../Scenarios/AuthCode", "dev")
    .WithReference(idsr)
    .WaitFor(idsr)
    .WithEnvironment("BROWSER", "none")
    .WithUrl("http://localhost:5173")
    .WithEnvironment("VITE_AUTHORITY", idsr.GetEndpoint("https"))
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
