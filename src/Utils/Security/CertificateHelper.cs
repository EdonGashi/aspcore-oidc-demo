using System;
using System.Security.Cryptography.X509Certificates;

namespace Utils.Security
{
    public class CertificateInfo
    {
        public string Subject { get; set; }
        public string StoreName { get; set; }
        public string StoreLocation { get; set; }
        public string FilePath { get; set; }
        public string Password { get; set; }
    }

    public static class CertificateHelper
    {
        public static X509Certificate2 LoadCertificate(CertificateInfo info, bool isDevelopment)
        {
            if (info.FilePath != null && info.Password != null)
            {
                return new X509Certificate2(info.FilePath, info.Password);
            }

            if (info.StoreName != null && info.StoreLocation != null)
            {
                using (var store = new X509Store(info.StoreName, Enum.Parse<StoreLocation>(info.StoreLocation)))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certificate = store.Certificates.Find(
                        X509FindType.FindBySubjectName,
                        info.Subject,
                        validOnly: !isDevelopment);

                    if (certificate.Count == 0)
                    {
                        throw new InvalidOperationException($"Certificate not found for {info.Subject}.");
                    }

                    return certificate[0];
                }
            }

            throw new InvalidOperationException("No valid certificate found.");
        }
    }
}
