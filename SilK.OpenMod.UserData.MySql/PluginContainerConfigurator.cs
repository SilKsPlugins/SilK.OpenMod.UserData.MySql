extern alias JetBrainsAnnotations;
using JetBrainsAnnotations::JetBrains.Annotations;
using OpenMod.API.Plugins;
using OpenMod.EntityFrameworkCore.MySql.Extensions;
using SilK.OpenMod.UserData.MySql.Database;

namespace SilK.OpenMod.UserData.MySql
{
    [UsedImplicitly]
    internal class PluginContainerConfigurator : IPluginContainerConfigurator
    {
        public void ConfigureContainer(IPluginServiceConfigurationContext context)
        {
            context.ContainerBuilder.AddMySqlDbContext<UserDataDbContext>();
        }
    }
}
