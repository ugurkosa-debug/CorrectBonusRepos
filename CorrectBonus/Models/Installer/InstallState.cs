namespace CorrectBonus.Models.Installer
{
    public class InstallState
    {
        // STEP 1 - LANGUAGE
        public string? Language { get; set; }

        // STEP 2 - SERVER
        public string? Server { get; set; }
        public bool ServerConnected { get; set; }

        // STEP 3 - DB LOGIN (ileride)
        public string? DbUser { get; set; }
        public bool DbAuthenticated { get; set; }

        // STEP 4 - DATABASE (ileride)
        public string? DatabaseName { get; set; }
        public bool DatabaseCreated { get; set; }

        // ORTAK
        public string? ErrorMessage { get; set; }
        // STEP 6 - OWNER
        public string? OwnerEmail { get; set; }
        public bool OwnerCreated { get; set; }
        public string? DbPassword { get; set; }
        public bool MailTested { get; set; }
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUser { get; set; }
        public string? SmtpPassword { get; set; }
        public bool EnableSsl { get; set; }


    }
}
