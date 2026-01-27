using CorrectBonus.Entities.Authorization;
using CorrectBonus.Entities.Common;
using CorrectBonus.Entities.Regions;
using CorrectBonus.Entities.System;
using CorrectBonus.Services.Auth;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CorrectBonus.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly CurrentUserContext? _currentUser;

        // 🔑 Runtime TenantId (nullable)
        public int? CurrentTenantId => _currentUser?.TenantId;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            CurrentUserContext? currentUser = null)
            : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
        public DbSet<Log> Logs => Set<Log>();
        public DbSet<UserPasswordHistory> UserPasswordHistories => Set<UserPasswordHistory>();
        public DbSet<RoleActionPermission> RoleActionPermissions { get; set; }
        public DbSet<TenantLicense> TenantLicenses { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<RegionType> RegionTypes { get; set; }
        public DbSet<SystemSeedHistory> SystemSeedHistories => Set<SystemSeedHistory>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Region>()
                .HasOne(r => r.RegionType)
                .WithMany()
                .HasForeignKey(r => r.RegionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Region>()
                .HasOne(r => r.ParentRegion)
                .WithMany(r => r.Children)
                .HasForeignKey(r => r.ParentRegionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Region>(entity =>
            {
                entity.Property(x => x.Coefficient)
                      .HasPrecision(18, 4);

                entity.Property(x => x.TargetValue)
                      .HasPrecision(18, 4);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
            });
            // ===============================
            // RBAC CONFIG (MEVCUT)
            // ===============================
            modelBuilder.Entity<RolePermission>()
                .HasKey(x => new { x.RoleId, x.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(x => x.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(x => x.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(x => x.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(x => x.PermissionId);

            modelBuilder.Entity<RoleActionPermission>()
                .HasIndex(x => new { x.RoleId, x.PermissionId, x.Action })
                .IsUnique();

            modelBuilder.Entity<RoleActionPermission>()
                .HasOne(x => x.Role)
                .WithMany(r => r.RoleActionPermissions)
                .HasForeignKey(x => x.RoleId);

            modelBuilder.Entity<RoleActionPermission>()
                .HasOne(x => x.Permission)
                .WithMany()
                .HasForeignKey(x => x.PermissionId);

            // ===============================
            // 🌍 REGION CONFIG
            // ===============================
            modelBuilder.Entity<Region>()
                .HasOne(x => x.ParentRegion)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentRegionId)
                .OnDelete(DeleteBehavior.Restrict);



            // ===============================
            // 🌍 GLOBAL TENANT QUERY FILTER (SAFE)
            // ===============================
            ApplyTenantQueryFilter(modelBuilder);
        }

        private void ApplyTenantQueryFilter(ModelBuilder modelBuilder)
        {
            var excludedTypes = new[]
            {
        typeof(RolePermission),
        typeof(RoleActionPermission)
    };

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                    continue;

                if (excludedTypes.Contains(entityType.ClrType))
                    continue;

                var parameter = Expression.Parameter(entityType.ClrType, "e");

                var tenantProperty = Expression.Convert(
                    Expression.Property(parameter, nameof(ITenantEntity.TenantId)),
                    typeof(int?)
                );

                var contextTenant = Expression.Property(
                    Expression.Constant(this),
                    nameof(CurrentTenantId)
                );

                var filterExpression = Expression.OrElse(
                    Expression.Equal(contextTenant, Expression.Constant(null, typeof(int?))),
                    Expression.Equal(tenantProperty, contextTenant)
                );

                var lambda = Expression.Lambda(filterExpression, parameter);

                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(lambda);
            }
        }

    }
}
