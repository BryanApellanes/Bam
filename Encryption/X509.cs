using Bam.Net.Encryption;
using Bam.Net.Logging;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Encryption
{
    public static class X509
    {
        public static X509Certificate2 Convert(this Org.BouncyCastle.X509.X509Certificate certificate)
        {
            return new X509Certificate2(certificate.GetEncoded());
        }

        public static X509Certificate2 Load(string filePath)
        {
            return new X509Certificate2(File.ReadAllBytes(filePath));
        }

        public static void CreateCertificateFile(string filePath, string subjectName, int validForYears = 2, string signatureAlgorithm = "SHA512WITHRSA")
        {
            Org.BouncyCastle.X509.X509Certificate generatedCertificate = GenerateSelfSignedCertificate(subjectName, validForYears, signatureAlgorithm);
            File.WriteAllBytes(filePath, generatedCertificate.GetEncoded());
            Log.AddEntry("Wrote certificate file: {0}", filePath);
        }

        public static Org.BouncyCastle.X509.X509Certificate GenerateSelfSignedCertificate(string subjectName, int validForYears = 2, string signatureAlgorithm = "SHA512WITHRSA")
        {
            SecureRandom random = SecureRandom.GetInstance("SHA256PRNG");
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            X509Name subjectDN = new X509Name(subjectName);
            X509Name issuerDN = subjectDN;
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            DateTime notBefore = DateTime.UtcNow.Date;
            DateTime notAfter = notBefore.AddYears(validForYears);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            AsymmetricCipherKeyPair subjectKeyPair = RsaKeyGen.GenerateKeyPair(RsaKeyLength._2048);
            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            AsymmetricCipherKeyPair issuerKeyPair = subjectKeyPair;
            ISignatureFactory signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, issuerKeyPair.Private, random);
            Org.BouncyCastle.X509.X509Certificate certificate = certificateGenerator.Generate(signatureFactory);
            return certificate;
        }
    }
}
