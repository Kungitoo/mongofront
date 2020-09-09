using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace mongoback
{
    public class MySession: MyDatabaseObject
    {
        public string ClientSessionId { get; set; }

        public string UserId { get; set; }
    }
}
