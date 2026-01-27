using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.System
{
    public class TenantDetailVm
    {
        public int Id { get; set; }

        // ======================
        // COMPANY
        // ======================
        [Required]
        public string Name { get; set; } = null!;

        public string? TaxNumber { get; set; }
        public string? Address { get; set; }

        // ======================
        // AUTHORIZED USER
        // ======================
        public int AuthorizedUserId { get; set; }

        [Required]
        public string? AuthorizedFullName { get; set; } = null!;

        [Required]
        public string? AuthorizedEmail { get; set; } = null!;

        public string? AuthorizedPhone { get; set; }

        // ======================
        // INVOICE
        // ======================
        public string? InvoiceTitle { get; set; }
        public string? InvoiceEmail { get; set; }
    }
}
