using System.ComponentModel.DataAnnotations.Schema;

namespace CorrectBonus.Entities.System
{
    [Table("UserPasswordHistories", Schema = "dbo")]
    public class UserPasswordHistory
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public byte[] PasswordHash { get; set; } = null!;
        public byte[] PasswordSalt { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
