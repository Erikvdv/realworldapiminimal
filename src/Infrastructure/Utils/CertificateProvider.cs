using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Realworlddotnet.Infrastructure.Utils.Interfaces;

namespace Realworlddotnet.Infrastructure.Utils;

public class CertificateProvider(ILogger<CertificateProvider> logger)
    : ICertificateProvider
{
    public X509Certificate2 LoadFromUserStore(string thumbprint)
    {
        if (string.IsNullOrWhiteSpace(thumbprint))
        {
            throw new ArgumentNullException(nameof(thumbprint));
        }

        logger.LogInformation("Loading certificate {Thumbprint} from store", thumbprint);

        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);

        var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

        store.Close();

        if (certCollection.Count <= 0)
        {
            throw new ArgumentException($"Unable to locate any certificate with thumbprint {thumbprint}.");
        }

        return certCollection[0];
    }

    public X509Certificate2 LoadFromFile(string filename, string password)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentNullException(nameof(filename));
        }

        logger.LogInformation("Loading certificate {Thumbprint}", filename);
        
        var certificate = X509CertificateLoader.LoadPkcs12FromFile(filename, password);

        if (certificate == null)
        {
            throw new ArgumentException("Unable to locate any certificate");
        }

        return certificate;
    }
}
