using System;
using System.Collections.Generic;
using System.Text;

namespace mongoback
{
    public enum TablePermissions
    {
        None = 0,
        CanRead = 1,
        CanWrite = 2,
    }

    public enum UserRoles
    {
        InvalidRole,
        Anonymous,
        RegisteredUser,
        PerCollectionAdmin,
        GlobalAdmin
    }

    public static class EnumUtilities
    {
        public static TablePermissions GetPermission(this UserRoles roles)
        {
            switch (roles)
            {
                case UserRoles.RegisteredUser:
                    return TablePermissions.CanRead;

                case UserRoles.PerCollectionAdmin:
                case UserRoles.GlobalAdmin:
                    return TablePermissions.CanRead | TablePermissions.CanWrite;

                case UserRoles.InvalidRole:
                case UserRoles.Anonymous:
                default:
                    return TablePermissions.None;
            }
        }
    }
}
