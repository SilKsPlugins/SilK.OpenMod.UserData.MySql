using System;

namespace SilK.OpenMod.UserData.MySql.Database.Models
{
    [Serializable]
    public class UserDataBanModel : UserDataForeignBase
    {
        public DateTime? ExpireDate { get; set; }

        public string? InstigatorType { get; set; }

        public string? InstigatorId { get; set; }

        public string? Reason { get; set; }
    }
}
