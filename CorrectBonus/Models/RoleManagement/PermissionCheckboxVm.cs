namespace CorrectBonus.Models.RoleManagement
{
    public class PermissionCheckboxVm
    {
        public int PermissionId { get; set; }
        public string Code { get; set; } = null!;
        // === ÇOK DİLLİ GÖSTERİM ===
        public string NameTr { get; set; } = null!;
        public string NameEn { get; set; } = null!;

        public string ModuleTr { get; set; } = null!;
        public string ModuleEn { get; set; } = null!;
        public bool IsSelected { get; set; }
        public bool CanList { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

    }

}
