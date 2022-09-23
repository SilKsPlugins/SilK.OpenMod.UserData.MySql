using System;

namespace SilK.OpenMod.UserData.MySql.Database.Models
{
    [Serializable]
    public abstract class UserDataKeyBase
    {
        public string Id { get; set; } = "";

        public string Type { get; set; } = "";
    }
}
