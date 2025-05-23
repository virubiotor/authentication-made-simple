using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql", port: 1433)
                 .WithLifetime(ContainerLifetime.Persistent)
                 .AddDatabase("Idsrdb");

var idsr = builder.AddProject<Projects.IdentityServer>("Idsr")
       .WithReference(sql)
       .WaitFor(sql)
       .WithExternalHttpEndpoints();

var consumerApi = builder.AddProject<Projects.ConsumerApi>("ClientCredentialsConsumerApi")
       .WithReference(idsr)
       .WaitFor(idsr)
       .WithExternalHttpEndpoints();

builder.AddProject<Projects.ClientApi>("ClientCredentialsClient")
       .WithReference(idsr)
       .WithReference(consumerApi)
       .WaitFor(consumerApi)
       .WithExternalHttpEndpoints();

var dpopApi = builder.AddProject<Projects.DPoP_Api>("DPopApi")
       .WaitFor(idsr)
       .WithReference(idsr)
       .WithExternalHttpEndpoints();

builder.AddProject<Projects.DPoP_Bff>("DPopBff")
       .WithReference(idsr)
       .WithReference(dpopApi)
       .WaitFor(dpopApi)
       .WithExternalHttpEndpoints();

var bffApi = builder.AddProject<Projects.BFF_Api>("BFFApi")
       .WaitFor(idsr)
       .WithReference(idsr)
       .WithExternalHttpEndpoints();

builder.AddProject<Projects.BFF>("BFF")
       .WithReference(idsr)
       .WithReference(bffApi)
       .WaitFor(bffApi)
       .WithExternalHttpEndpoints();

builder.AddNpmApp("AuthCode", "../../Scenarios/AuthCode", "dev")
    .WithReference(idsr)
    .WaitFor(idsr)
    .WithEnvironment("BROWSER", "none")
    .WithUrl("http://localhost:5173")
    .WithEnvironment("VITE_AUTHORITY", idsr.GetEndpoint("https"))
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
