using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CorrectBonus.Services.Licensing
{
    public class LicenseGeneratorService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public LicenseGeneratorService(
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        /// <summary>
        /// Uzun (imzalı) lisansı üretir: CBX1.payload.signature
        /// </summary>
        public string GenerateSignedLicense(LicensePayload payload)
        {
            var relativePath = _configuration["License:PrivateKeyPath"];

            if (string.IsNullOrWhiteSpace(relativePath))
                throw new InvalidOperationException("License:PrivateKeyPath is not configured.");

            var fullPath = Path.Combine(_env.ContentRootPath, relativePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Private key file not found.", fullPath);

            var pem = File.ReadAllText(fullPath);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(pem);

            // 🔴 Payload deterministic & uyumlu
            var payloadJson = JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
            var payloadBase64 = Convert.ToBase64String(payloadBytes);

            var signatureBytes = rsa.SignData(
                payloadBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            var signatureBase64 = Convert.ToBase64String(signatureBytes);

            return $"CBX1.{payloadBase64}.{signatureBase64}";
        }

        /// <summary>
        /// Kullanıcıya gösterilecek KISA lisans anahtarı (Windows key gibi)
        /// </summary>
        public string GeneratePublicKey()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            string Part()
            {
                var bytes = new byte[5];
                RandomNumberGenerator.Fill(bytes);
                return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
            }

            return $"CBX1-{Part()}-{Part()}-{Part()}-{Part()}";
        }
    }
}
