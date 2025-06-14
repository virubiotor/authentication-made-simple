using PoPmTLS.Client;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddHttpClient("apiClient",
    client => client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("apiUrl")!))
    .ConfigurePrimaryHttpMessageHandler(CertificateHandler.GetHandler);

builder.Services.AddHttpClient("idpClient",
    client => client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("authority")!))
    .ConfigurePrimaryHttpMessageHandler(CertificateHandler.GetHandler);

var host = builder.Build();
host.Run();