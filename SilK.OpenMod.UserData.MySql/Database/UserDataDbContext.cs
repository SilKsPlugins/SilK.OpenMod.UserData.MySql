using Microsoft.EntityFrameworkCore;
using OpenMod.EntityFrameworkCore;
using OpenMod.EntityFrameworkCore.Configurator;
using SilK.OpenMod.UserData.MySql.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SilK.OpenMod.UserData.MySql.Database
{
    public class UserDataDbContext : OpenModDbContext<UserDataDbContext>
    {
        public UserDataDbContext(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public UserDataDbContext(IDbContextConfigurator configurator, IServiceProvider serviceProvider) : base(
            configurator, serviceProvider)
        {
        }

        public DbSet<UserDataModel> Users => Set<UserDataModel>();

        public DbSet<UserDataBanModel> UserBans => Set<UserDataBanModel>();

        public DbSet<UserDataPermissionModel> UserPermissions => Set<UserDataPermissionModel>();

        public DbSet<UserDataRoleModel> UserRoles => Set<UserDataRoleModel>();

        public DbSet<UserDataObjectModel> UserDataObjects => Set<UserDataObjectModel>();

        protected override string TablePrefix => "OpenModUserData_";

        protected override string MigrationsTableName => "__OpenModUserData_MigrationsHistory";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserDataModel>(entity =>
            {
                void SetupOneToOne<T>(Expression<Func<UserDataModel, T?>> navigationExpression) where T : UserDataForeignBase
                {
                    entity.HasOne(navigationExpression)
                        .WithOne(x => x!.UserData)
                        .HasForeignKey<T>(x => new { x.Id, x.Type })
                        .OnDelete(DeleteBehavior.Cascade);
                }

                void SetupOneToMany<T>(Expression<Func<UserDataModel, IEnumerable<T>>> navigationExpression) where T : UserDataForeignBase
                {
                    entity.HasMany(navigationExpression)
                        .WithOne(x => x!.UserData)
                        .HasForeignKey(x => new { x.Id, x.Type })
                        .OnDelete(DeleteBehavior.Cascade);
                }

                entity.HasKey(x => new { x.Id, x.Type });

                SetupOneToOne(x => x.BanInfo);

                SetupOneToMany(x => x.Permissions);
                SetupOneToMany(x => x.Roles);
                SetupOneToMany(x => x.Data);
            });

            modelBuilder.Entity<UserDataBanModel>()
                .HasKey(x => new { x.Id, x.Type });

            modelBuilder.Entity<UserDataPermissionModel>()
                .HasKey(x => new { x.Id, x.Type, x.Permission });

            modelBuilder.Entity<UserDataRoleModel>()
                .HasKey(x => new { x.Id, x.Type, x.Role });

            modelBuilder.Entity<UserDataObjectModel>(entity =>
            {
                entity.HasKey(x => new { x.Id, x.Type, x.Key });

                entity.Property(x => x.Object)
                    .HasColumnType("json");
            });
        }
    }
}
