using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace mongoback
{
    public class MyMongoService
    {
        public const string LOCALHOST_MONGO = "mongodb://localhost";
        public const string USERCOLLECTION = "myUsers";
        public const string SESSIONCOLLECTION = "sessions";
        public const string ADMINDB = "myAdmin";
        public const string TESTADMINDB = "myTestAdmin";

        public MyMongoService(string connectionString, bool isTest)
        {
            this.IsTest = isTest;
            this.ConnectionString = connectionString;

            this.client = new MongoClient(ConnectionString);

            this.AdminDb = client.GetDatabase(isTest ? TESTADMINDB : ADMINDB);
        }

        public bool IsTest { get; }
        public string ConnectionString { get; }
        public string DatabaseName { get; }

        private MongoClient client;

        public IMongoDatabase AdminDb { get; }

        public List<BsonDocument> GetResources(string path, string sessionId)
        {
            var split = path.Split('/');

            string databaseName = split[0];
            string collectionName = split[1];
            string resourceName = split.Length == 3 ? split[3] : null;

            UserRoles userRoles = GetRoleFromSessionId(sessionId, databaseName);

            if ((userRoles.GetPermission() & TablePermissions.CanRead) != 0)
            {
                var database = client.GetDatabase(databaseName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                IFindFluent<BsonDocument, BsonDocument> objects = collection.Find(x => resourceName == null ? true : (x["Id"] == resourceName));
                List<BsonDocument> list = objects.ToList();
                return list;
            }
            else
            {
                return new List<BsonDocument>();
            }
        }

        public bool RegisterUser(string email, string password, string database)
        {
            var usersColl = GetUsersColl();

            if (usersColl.Find(u => u.Email == email).Any())
            {
                return false;
            }

            var hashed = BCrypt.Net.BCrypt.HashPassword(password);
            var newUser = new MyUser();
            newUser.DatabaseOrigin = database;
            newUser.HashedPassword = hashed;
            newUser.Email = email;
            newUser.VerifiedEmail = false;

            usersColl.InsertOne(newUser);

            return true;
        }

        private IMongoCollection<MyUser> GetUsersColl()
        {
            return AdminDb.GetCollection<MyUser>(USERCOLLECTION);
        }

        public void LoginUser(string email, string password, string database, string sessionId)
        {
            var session = GetSession(sessionId);
            var usersColl = GetUsersColl();

            var tryGetUserFromSession = GetUserForDatabase(database, session.UserId);

            if (tryGetUserFromSession != null) return; // Already loged in I guess

            var theUser = usersColl.Find(u => u.Email == email).Single();
            if (BCrypt.Net.BCrypt.EnhancedVerify(password, theUser.HashedPassword))
            {
                session.UserId = theUser.Id;
                var sessionColl = GetSessionColl();
                sessionColl.ReplaceOne(u => u.Id == session.Id, session);

                // TODO: TEST
            }
        }

        private UserRoles GetRoleFromSessionId(string sessionId, string databaseName)
        {

            // First authorize
            MySession session = GetSession(sessionId);

            UserRoles userRoles = UserRoles.InvalidRole;
            if (session.UserId != null) userRoles = UserRoles.Anonymous;

            string userId = session.UserId;
            MyUser userFromDb = GetUserForDatabase(databaseName, userId);

            if (userFromDb != null) userRoles = userFromDb.Role;

            return userRoles;
        }

        private MyUser GetUserForDatabase(string databaseName, string userId)
        {
            MyUser userFromDb = null;

            if (userId != null)
            {
                var userColl = AdminDb.GetCollection<MyUser>(USERCOLLECTION);

                var usersFromDb = userColl.Find(u => u.Id == userId);

                if (usersFromDb.Any())
                {
                    userFromDb = usersFromDb.First();

                    if (userFromDb.DatabaseOrigin != databaseName && userFromDb.Role != UserRoles.GlobalAdmin)
                    {
                        throw new InvalidOperationException("There has to be exactly 1 session in the db with the same key");
                    }

                }
            }

            return userFromDb;
        }

        public MySession GetSession(string sessionId)
        {
            MySession session;

            var sessionColl = GetSessionColl();
            var sessionsFromDb = sessionColl.Find(s => s.ClientSessionId == sessionId);

            // If session already exists in the db
            if (sessionsFromDb.Any())
            {
                if (sessionsFromDb.CountDocuments() != 1) throw new InvalidOperationException("There has to be exactly 1 session in the db with the same key");

                session = sessionsFromDb.First();
            }
            else
            {
                session = new MySession();
                session.ClientSessionId = sessionId;

                sessionColl.InsertOne(session);
            }

            return session;
        }

        private IMongoCollection<MySession> GetSessionColl()
        {
            return AdminDb.GetCollection<MySession>(SESSIONCOLLECTION);
        }
    }
}
