namespace CorrectBonus.Services.Licensing
{
    public interface ILicenseService
    {
        Task<LicenseState> GetCurrentTenantLicenseStateAsync();
    }
}