using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StackExchange.Redis.WordQueryTest
{
    [TestClass]
    public abstract class BaseTest
    {
        protected string QueryableHashKey = "WQ:::Queryable";
        protected string QueryableDataHashKey = "WQ:::QueryableData";
        protected string QueryHashKey = "WQ:::Query";

        private const int DATABASENUMBER = AppSettings.REDISDBNUMBER;
        private const string HOST = AppSettings.REDISHOST;
        private const string PASSWORD = AppSettings.REDISPASSWORD;

        protected static ConnectionMultiplexer Connection { get { return lazyConnection.Value;/*throw new Exception("Disabled");*/} }

        protected static IDatabase Database { get { return Connection.GetDatabase(DATABASENUMBER); } }
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() => {

            return ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                Password = PASSWORD,
                AllowAdmin = true,
                AbortOnConnectFail = false,
                SyncTimeout = 3000,
                EndPoints =
            {
                { HOST, 6379 },
            },
            });

        });
        [AssemblyInitializeAttribute]
        public static void Initialize(TestContext context)
        {
            Assert.IsTrue(Connection.IsConnected);
            FlushDB();

        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            Console.WriteLine("Cleanup executed.");
            //FlushDB();
        }

        [TestInitialize()]
        public void SetupTest()
        {
            FlushDB();
        }

        [TestCleanup()]
        public void CleanupTest()
        {
            ;
        }
        public static void FlushDB()
        {
            Connection.GetServer(HOST, 6379).FlushDatabase(DATABASENUMBER);
        }
    }
}
