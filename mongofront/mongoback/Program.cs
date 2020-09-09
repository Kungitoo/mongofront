using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace mongoback
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            // ...
            var client = new MongoClient(
                "mongodb+srv://<username>:<password>@<cluster-address>/test?w=majority"
            );
            var database = client.GetDatabase("test");
        }
    }
}
