namespace SilK.OpenMod.UserData.MySql.Database.Models
{
    public class UserDataObjectModel : UserDataForeignBase
    {
        public string Key { get; set; } = "";

        public string? Object { get; set; }
    }
}
