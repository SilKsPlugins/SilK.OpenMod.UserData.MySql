using System;
using System.Collections.Generic;

namespace SilK.OpenMod.UserData.MySql.Database.Models
{
    [Serializable]
    public class UserDataModel : UserDataKeyBase
    {
        public string? LastDisplayName { get; set; }

        public DateTime? FirstSeen { get; set; }

        public DateTime? LastSeen { get; set; }

        public UserDataBanModel? BanInfo { get; set; }

        public IList<UserDataPermissionModel> Permissions { get; set; } = new List<UserDataPermissionModel>();

        public IList<UserDataRoleModel> Roles { get; set; } = new List<UserDataRoleModel>();

        public IList<UserDataObjectModel> Data { get; set; } = new List<UserDataObjectModel>();
    }
}
