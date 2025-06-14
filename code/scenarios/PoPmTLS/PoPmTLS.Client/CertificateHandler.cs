using System.Security.Cryptography.X509Certificates;

namespace PoPmTLS.Client;

public static class CertificateHandler
{
    public static SocketsHttpHandler GetHandler()
    {
        var handler = new SocketsHttpHandler();

        // When running from Visual Studio the current directory gets set to the assembly
        // location, but when running from command prompt with dotnet run the current
        // directory is likely the project directory. This works for both.
        // This assumes the certificate is in the root of the repository.
        var assemblyDir = typeof(Worker).Assembly.Location;
        var certPath = Path.GetFullPath(Path.Combine(assemblyDir, "../../../../../../../../localhost-client.p12"));

        var cert = new X509Certificate2(certPath, "changeit");
        handler.SslOptions.ClientCertificates = new X509CertificateCollection { cert };

        return handler;
    }
}