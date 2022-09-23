extern alias JetBrainsAnnotations;
using JetBrainsAnnotations::JetBrains.Annotations;
using OpenMod.EntityFrameworkCore.MySql;

namespace SilK.OpenMod.UserData.MySql.Database
{
    [UsedImplicitly]
    internal class UserDataDbContextFactory : OpenModMySqlDbContextFactory<UserDataDbContext>
    {
    }
}
