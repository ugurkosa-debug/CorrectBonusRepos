using ClosedXML.Excel;
using CorrectBonus.Attributes;
using CorrectBonus.Data;
using CorrectBonus.Models.LogManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers
{
    [Authorize]
    [RequirePermission(PermissionRegistry.Logs.View)]
    public class LogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================================================
        // LIST (PAGE)
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> Index(LogListVm model)
        {
            var page = model.Page <= 0 ? 1 : model.Page;
            var pageSize = model.PageSize <= 0 ? 20 : model.PageSize;

            var query = _context.Logs
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.Level))
                query = query.Where(x => x.Level == model.Level);

            if (!string.IsNullOrWhiteSpace(model.ActionCode))
                query = query.Where(x => x.Action.Contains(model.ActionCode));

            if (!string.IsNullOrWhiteSpace(model.UserEmail))
                query = query.Where(x => x.UserEmail != null &&
                                         x.UserEmail.Contains(model.UserEmail));

            model.TotalCount = await query.CountAsync();

            model.Logs = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            model.Page = page;
            model.PageSize = pageSize;

            return View(model);
        }

        // ==================================================
        // EXPORT (ACTION)
        // ==================================================
        [HttpGet]
        [RequirePermission(PermissionRegistry.Logs.Export)]
        public async Task<IActionResult> Export(
            string? level,
            string? actionCode,
            string? userEmail)
        {
            var query = _context.Logs
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(level))
                query = query.Where(x => x.Level == level);

            if (!string.IsNullOrWhiteSpace(actionCode))
                query = query.Where(x => x.Action.Contains(actionCode));

            if (!string.IsNullOrWhiteSpace(userEmail))
                query = query.Where(x => x.UserEmail != null &&
                                         x.UserEmail.Contains(userEmail));

            var logs = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Logs");

            ws.Cell(1, 1).Value = "Tarih";
            ws.Cell(1, 2).Value = "Seviye";
            ws.Cell(1, 3).Value = "Aksiyon";
            ws.Cell(1, 4).Value = "E-Posta";
            ws.Cell(1, 5).Value = "Mesaj";
            ws.Range(1, 1, 1, 5).Style.Font.Bold = true;

            var row = 2;
            foreach (var log in logs)
            {
                ws.Cell(row, 1).Value =
                    log.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
                ws.Cell(row, 2).Value = log.Level;
                ws.Cell(row, 3).Value = log.Action;
                ws.Cell(row, 4).Value = log.UserEmail;
                ws.Cell(row, 5).Value = log.Message;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Logs_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            );
        }
    }
}
