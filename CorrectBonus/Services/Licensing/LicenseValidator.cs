using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CorrectBonus.Services.Licensing
{
    public class LicenseValidator
    {
        private readonly string _publicKey;

        public LicenseValidator(IWebHostEnvironment env)
        {
            var keyPath = Path.Combine(env.WebRootPath, "license_public.pem");

            if (!File.Exists(keyPath))
                throw new InvalidOperationException(
                    "license_public.pem not found. License validation cannot start.");

            _publicKey = File.ReadAllText(keyPath);
        }

        public LicensePayload? Validate(string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                return null;

            try
            {
                // CBX1.payload.signature
                var parts = licenseKey.Split('.');
                if (parts.Length != 3 || parts[0] != "CBX1")
                    return null;

                var payloadJson = Encoding.UTF8.GetString(
                    Convert.FromBase64String(parts[1]));

                var signature = Convert.FromBase64String(parts[2]);

                using var rsa = RSA.Create();
                rsa.ImportFromPem(_publicKey);

                var isValid = rsa.VerifyData(
                    Encoding.UTF8.GetBytes(payloadJson),
                    signature,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                if (!isValid)
                    return null;

                var payload = JsonSerializer.Deserialize<LicensePayload>(
                    payloadJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (payload == null)
                    return null;

                // 🔴 KRİTİK VALIDASYONLAR
                if (payload.Exp <= DateTime.UtcNow)
                    return null;

                if (string.IsNullOrWhiteSpace(payload.Tenant))
                    return null;

                return payload;
            }
            catch
            {
                // TODO: log
                return null;
            }
        }
    }
}
