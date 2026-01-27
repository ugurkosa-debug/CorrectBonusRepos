using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.Installer
{
    public class InstallerWizardVm
    {
        // =====================
        // WIZARD
        // =====================
        public int Step { get; set; } = 1;
        public bool ShowValidationErrors { get; set; } = false;

        // =====================
        // LANGUAGE
        // =====================
        [Required]
        public string SelectedLanguage { get; set; } = "tr";

        // =====================
        // DATABASE
        // =====================
        [Required(ErrorMessage = "Installer.Db.Server.Required")]
        public string Server { get; set; } = "";

        [Required(ErrorMessage = "Installer.Db.Database.Required")]
        public string Database { get; set; } = "";

        public string DbUser { get; set; } = "";
        public string DbPassword { get; set; } = "";

        public bool DbConnectionTested { get; set; } = false;
        public bool DbConnectionSuccess { get; set; } = false;

        // =====================
        // SYSTEM OWNER
        // =====================
        [Required(ErrorMessage = "Installer.Admin.Email.Required")]
        [EmailAddress(ErrorMessage = "Installer.Admin.Email.Invalid")]
        public string AdminEmail { get; set; } = "";
    }
}
