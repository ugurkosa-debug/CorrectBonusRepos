public class Tenant
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;

    public string? TaxNumber { get; set; }
    public string? Address { get; set; }

    public string? InvoiceTitle { get; set; }
    public string? InvoiceEmail { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
