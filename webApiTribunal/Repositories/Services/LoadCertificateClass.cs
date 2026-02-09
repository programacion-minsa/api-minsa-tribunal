using System.Security.Cryptography.X509Certificates;

namespace webApiTribunal.Repositories.Services;

public static class LoadCertificateClass
{
    public static X509Certificate2 LoadCertificateFromStore(string thumbprint, string password, string localCertPath)
    {
        X509Certificate2 certificate= new X509Certificate2();
        
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        
        var certificates = store.Certificates.Find(
            X509FindType.FindByThumbprint,
            thumbprint,
            false
        );

        if (certificates.Count == 0)
        {
            var certFile = new X509Certificate2(localCertPath, password);
            if (certFile != null)
            {
                certificate = certFile;
            }
        }
        else
        {
            certificate = certificates[0];
        }
        
        if (certificate == null)
        {
            throw new InvalidOperationException($"Certificate with thumbprint {thumbprint} not found.");
        }
        
        if (!certificate.HasPrivateKey)
        {
            throw new InvalidOperationException("Certificate found but does not contain a private key.");
        }
        
        //
        // // Check if the certificate has a private key for client authentication
        // if (!certificates[0].HasPrivateKey)
        // {
        //     throw new InvalidOperationException("Certificate found but does not contain a private key.");
        // }
        
        // var cert = new X509Certificate2(localCertPath, password);
        // if (cert == null)
        // {
        //     throw new InvalidOperationException($"Certificate with thumbprint {thumbprint} not found.");
        // }
        
        // if (!cert.HasPrivateKey)
        // {
        //     throw new InvalidOperationException("Certificate found but does not contain a private key.");
        // }
        
        return certificate;
    }
}