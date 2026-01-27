using ClosedXML.Excel;
using CorrectBonus.Entities.Regions;

namespace CorrectBonus.Services.Regions
{
    public class RegionExcelService
    {
        public byte[] Export(List<Region> regions)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Regions");

            ws.Cell(1, 1).Value = "Name";
            ws.Cell(1, 2).Value = "Type";
            ws.Cell(1, 3).Value = "ERP Code";

            int row = 2;
            foreach (var r in regions)
            {
                ws.Cell(row, 1).Value = r.Name;
                ws.Cell(row, 2).Value = r.RegionType;
                ws.Cell(row, 3).Value = r.ErpCode;
                row++;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
