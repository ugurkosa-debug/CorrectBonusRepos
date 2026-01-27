namespace CorrectBonus.Models.LogManagement
{
    public class LogListVm
    {
        // Filtreler
        public string? Level { get; set; }
        public string? ActionCode { get; set; }   // 🔥 Action DEĞİL
        public string? UserEmail { get; set; }

        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }

        // UI
        public bool ShowFilters { get; set; }

        // Liste
        public List<Entities.System.Log> Logs { get; set; } = new();
    }
}
