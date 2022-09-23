namespace SilK.OpenMod.UserData.MySql.Database.Models
{
    public abstract class UserDataForeignBase : UserDataKeyBase
    {
        public UserDataModel UserData { get; set; } = null!;
    }
}
