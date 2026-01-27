using CorrectBonus.Data;
using CorrectBonus.Middleware;
using CorrectBonus.Security;
using CorrectBonus.Services.Auth;
using CorrectBonus.Services.Licensing;
using CorrectBonus.Services.Localization;
using CorrectBonus.Services.Logs;
using CorrectBonus.Services.Mail;
using CorrectBonus.Services.Regions;
using CorrectBonus.Services.System;
using CorrectBonus.Services.System.Seeding;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ==================================================
// INSTALL FLAG
// ==================================================
var appDataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
var installedFlagPath = Path.Combine(appDataPath, "installed.flag");

Directory.CreateDirectory(appDataPath);
var isInstalled = File.Exists(installedFlagPath);

// ==================================================
// GENEL SERVİSLER
// ==================================================
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = isInstalled ? "/Auth/Login" : "/Installer";
        o.AccessDeniedPath = isInstalled ? "/Auth/AccessDenied" : "/Installer";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(); // ⬅️ TEK VE NET
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(8);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// ==================================================
// DATABASE
// ==================================================
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// ==================================================
// APPLICATION SERVİSLERİ
// ==================================================
if (isInstalled)
{

    // CORE CONTEXT
    builder.Services.AddScoped<CurrentUserContext>();

    // SYSTEM SETTINGS
    builder.Services.AddScoped<SystemSettingService>();
    builder.Services.AddScoped<ISystemSettingService>(sp =>
        sp.GetRequiredService<SystemSettingService>());

    // LOG
    builder.Services.AddScoped<ILogService, LogService>();

    // AUTH / SECURITY
    builder.Services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
    builder.Services.AddScoped<IPermissionService, PermissionService>();

    // LICENSE
    builder.Services.AddScoped<ILicenseService, LicenseService>();
    builder.Services.AddScoped<LicenseValidator>();
    builder.Services.AddScoped<LicenseGeneratorService>();
    builder.Services.AddScoped<LicenseWarningService>();
    builder.Services.AddHostedService<LicenseNotificationHostedService>();

    // REGION
    builder.Services.AddScoped<RegionService>();
    builder.Services.AddScoped<RegionExcelService>();

    // MAIL
    builder.Services.AddScoped<IMailService, MailService>();
    builder.Services.AddScoped<MailTemplateService>();

    // MENU
    builder.Services.AddScoped<MenuService>();

    // SYSTEM SEED
    builder.Services.AddScoped<SystemSeedRunner>();
    builder.Services.AddSingleton<MenuValidationService>();
    builder.Services.AddScoped<ISystemSeed, MenuSeed>();

    // LOCALIZATION
    builder.Services.AddScoped<ICommonLocalizer, CommonLocalizer>();


}

// ==================================================
// 🔐 AUTHORIZATION HANDLER (ÇOK KRİTİK)
// ==================================================
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// ==================================================
// BUILD
// ==================================================
var app = builder.Build();

// ==================================================
// PIPELINE
// ==================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// ==================================================
// LOCALIZATION
// ==================================================
var cultures = new[]
{
    new CultureInfo("tr-TR"),
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = cultures,
    SupportedUICultures = cultures,

    RequestCultureProviders = new IRequestCultureProvider[]
    {
        new CookieRequestCultureProvider()
    }
});

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ==================================================
// LICENSE MIDDLEWARE
// ==================================================
if (isInstalled)
{
    app.UseMiddleware<TenantLicenseMiddleware>();
}

// ==================================================
// ROUTING
// ==================================================
app.MapControllerRoute(
    name: "default",
    pattern: isInstalled
        ? "{controller=Auth}/{action=Login}/{id?}"
        : "{controller=Installer}/{action=Index}/{id?}"
);

// ==================================================
// 🔑 PERMISSION SEED (EN DOĞRU YER)
// ==================================================
if (isInstalled)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    PermissionSeed.Seed(db);
}

app.Run();
