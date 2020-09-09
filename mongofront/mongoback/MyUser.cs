using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace mongoback
{
    public class MyUser: MyDatabaseObject
    {
        public string Username { get; set; }

        /// <summary>
        /// To which database does the user belong to. Can be only 1
        /// </summary>
        public string DatabaseOrigin { get; set; }

        public UserRoles Role { get; set; }
        public string HashedPassword { get; internal set; }
        public string Email { get; internal set; }
        public bool VerifiedEmail { get; internal set; }
    }
}
