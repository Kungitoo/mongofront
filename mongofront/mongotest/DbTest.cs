using mongoback;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace mongotest
{
    public class DbTest
    {
        public DbTest()
        {

        }

        [Fact]
        public void AdminUserTest()
        {
            var adminSessionId = SetupSuperAdminWithSession();

            var mongo = new MyMongoService(MyMongoService.LOCALHOST_MONGO, isTest: true);

            var resource = mongo.GetResources(MyMongoService.TESTADMINDB + "/" + MyMongoService.USERCOLLECTION, adminSessionId);

            Assert.Single(resource);

            var first = resource.First();
            Assert.Equal(UserRoles.GlobalAdmin, first["Role"]);
        }

        private static string SetupSuperAdminWithSession()
        {
            var connectionString = MyMongoService.LOCALHOST_MONGO;

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(MyMongoService.TESTADMINDB);
            var userCollection = database.GetCollection<MyUser>(MyMongoService.USERCOLLECTION);

            var superAdmin = new MyUser();
            superAdmin.DatabaseOrigin = MyMongoService.TESTADMINDB;
            superAdmin.Role = UserRoles.GlobalAdmin;

            userCollection.DeleteMany(_ => true);
            userCollection.InsertOne(superAdmin);

            var sessionCollection = database.GetCollection<MySession>(MyMongoService.SESSIONCOLLECTION);

            var adminSession = new MySession();
            adminSession.ClientSessionId = Guid.NewGuid().ToString();
            adminSession.UserId = superAdmin.Id;

            sessionCollection.DeleteMany(_ => true);
            sessionCollection.InsertOne(adminSession);

            return adminSession.ClientSessionId;
        }
    }
}
