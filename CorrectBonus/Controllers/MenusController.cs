using CorrectBonus.Data;
using CorrectBonus.Entities.System;
using CorrectBonus.Models.MenuManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CorrectBonus.Authorization;

public class MenusController : Controller
{
    private readonly ApplicationDbContext _context;

    public MenusController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ===============================
    // LIST
    // ===============================
    public IActionResult Index()
    {
        var menus = _context.Menus
            .AsNoTracking()
            .OrderBy(m => m.Order)
            .Select(m => new MenuListVm
            {
                Id = m.Id,
                Title = m.Title,
                PermissionCode = m.PermissionCode ?? string.Empty,
                IsActive = m.IsActive
            })
            .ToList();

        return View(menus);
    }


    // ===============================
    // CREATE
    // ===============================
    public IActionResult Create()
    {
        return View(BuildEditVm(new MenuEditVm()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(MenuEditVm model)
    {
        if (!ModelState.IsValid)
            return View(BuildEditVm(model));

        var menu = new Menu
        {
            Title = model.Title,
            Controller = string.IsNullOrWhiteSpace(model.Controller)
                ? null
                : model.Controller,

            Action = string.IsNullOrWhiteSpace(model.Action)
                ? null
                : model.Action,

            Icon = string.IsNullOrWhiteSpace(model.Icon)
                ? null
                : model.Icon,

            Order = model.Order,
            ParentId = model.ParentId,

            PermissionCode = string.IsNullOrWhiteSpace(model.PermissionCode)
                ? null
                : model.PermissionCode,

            IsActive = model.IsActive
        };

        _context.Menus.Add(menu);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // ===============================
    // EDIT
    // ===============================
    public IActionResult Edit(int id)
    {
        var menu = _context.Menus.Find(id);
        if (menu == null)
            return NotFound();

        var vm = new MenuEditVm
        {
            Id = menu.Id,
            Title = menu.Title,

            Controller = menu.Controller ?? string.Empty,
            Action = menu.Action ?? string.Empty,
            Icon = menu.Icon ?? string.Empty,

            Order = menu.Order,
            ParentId = menu.ParentId,
            PermissionCode = menu.PermissionCode ?? string.Empty,
            IsActive = menu.IsActive
        };

        return View(BuildEditVm(vm));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(MenuEditVm model)
    {
        if (!ModelState.IsValid)
            return View(BuildEditVm(model));

        var menu = _context.Menus.Find(model.Id);
        if (menu == null)
            return NotFound();

        menu.Title = model.Title;

        menu.Controller = string.IsNullOrWhiteSpace(model.Controller)
            ? null
            : model.Controller;

        menu.Action = string.IsNullOrWhiteSpace(model.Action)
            ? null
            : model.Action;

        menu.Icon = string.IsNullOrWhiteSpace(model.Icon)
            ? null
            : model.Icon;

        menu.Order = model.Order;
        menu.ParentId = model.ParentId;

        menu.PermissionCode = string.IsNullOrWhiteSpace(model.PermissionCode)
            ? null
            : model.PermissionCode;

        menu.IsActive = model.IsActive;

        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    // ===============================
    // HELPERS
    // ===============================
    private MenuEditVm BuildEditVm(MenuEditVm vm)
    {
        vm.ParentMenus = _context.Menus
            .AsNoTracking()
            .Where(m => m.ParentId == null)
            .OrderBy(m => m.Title)
            .ToList();

        vm.Permissions = _context.Permissions
            .AsNoTracking()
            .OrderBy(p => p.NameTr)
            .ToList();

        return vm;
    }
}
