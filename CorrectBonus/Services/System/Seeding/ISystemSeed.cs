using CorrectBonus.Data;

namespace CorrectBonus.Services.System.Seeding
{
    public interface ISystemSeed
    {
        string Version { get; }
        Task ApplyAsync(ApplicationDbContext db);
    }
}
