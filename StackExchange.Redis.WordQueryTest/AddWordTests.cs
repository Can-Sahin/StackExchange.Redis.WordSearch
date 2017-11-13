using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StackExchange.Redis.WordQuery;
using StackExchange.Redis.WordQuery.Model;
using System.Text;

namespace StackExchange.Redis.WordQueryTest
{
    [TestClass]
    public class AddWordTests:BaseTest
    {
        [TestMethod]
        public void Add_QueryableWord()
        {
            string queryWord = "testValue";
            string queryWordId = "testId";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord);
            
            string value = Database.HashGet(QueryableHashKey, queryWordId);
            Assert.AreEqual(queryWord, value);
        }
        [TestMethod]
        public void Add_QueryableWord_With_Data()
        {
            string queryWord = "testValue";
            string queryWordId = "testId";
            string data = "testData";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord,data);

            string value = Database.HashGet(QueryableDataHashKey, queryWordId);
            Assert.AreEqual(data, value);
        }
        [TestMethod]
        public void Add_QueryableWord_Update_Data()
        {
            string queryWord = "testValue";
            string queryWordId = "testId";
            string dataFirst = "testData";
            string dataSecond = "testData2";

            FlushDB();

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord, dataFirst);

            wordQuery.Add(queryWordId, queryWord, dataSecond);

            string value = Database.HashGet(QueryableDataHashKey, queryWordId);
            Assert.AreEqual(dataSecond, value);
        }
        [TestMethod]
        public void Add_QueryableWord_Object()
        {
            string queryWord = "testValue";
            string queryWordId = "testId";
            TestObject dataObject = new TestObject("someData") ;

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.Serializer = new TestJsonSerializer();

            RedisWordQuery wordQuery = new RedisWordQuery(Database,config);
            wordQuery.AddObject(queryWordId, queryWord, dataObject);

            byte[] value = Database.HashGet(QueryableDataHashKey, queryWordId);
            Assert.AreEqual(dataObject.aProperty, (new TestJsonSerializer().Deserialize<TestObject>(value)).aProperty);

        }
        [TestMethod]
        public void Add_QueryableWord_RemoveAfter()
        {
            string queryWord = "testValue";
            string queryWordId = "testId";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord);

            string value = Database.HashGet(QueryableHashKey, queryWordId);
            Assert.AreEqual(queryWord, value);

            wordQuery.Remove(queryWordId);
            Assert.AreEqual(true, Database.HashGet(QueryableHashKey, queryWordId).IsNull);

        }
     
    }
}
