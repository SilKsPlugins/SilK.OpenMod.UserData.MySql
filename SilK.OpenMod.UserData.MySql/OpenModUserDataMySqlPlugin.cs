extern alias JetBrainsAnnotations;
using Autofac;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using OpenMod.API.Plugins;
using OpenMod.Core.Plugins;
using SilK.OpenMod.UserData.MySql.Database;
using System;
using System.Threading.Tasks;

[assembly: PluginMetadata("SilK.OpenMod.UserData.MySql", DisplayName = "SilK's MySQL UserData", Author = "SilK")]

namespace SilK.OpenMod.UserData.MySql
{
    [UsedImplicitly]
    public class OpenModUserDataMySqlPlugin : OpenModUniversalPlugin
    {
        public OpenModUserDataMySqlPlugin(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task OnLoadAsync()
        {
            await using var dbContext = LifetimeScope.Resolve<UserDataDbContext>();

            await dbContext.Database.MigrateAsync();
        }
    }
}