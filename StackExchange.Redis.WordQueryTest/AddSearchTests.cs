using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StackExchange.Redis.WordQuery;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace StackExchange.Redis.WordQueryTest
{
    [TestClass]
    public class AddSearchTests : BaseTest
    {
        [TestMethod]
        [DataRow("testValue")]
        public void AddWord(string queryWord)
        {
            string queryWordId = "testId";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            Assert.IsTrue(wordQuery.Add(queryWordId, queryWord));
        }
        [TestMethod]
        [DataRow("testValue")]
        public void UpdateWord(string queryWord)
        {
            string queryWordId = "testId";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            Assert.IsTrue(wordQuery.Add(queryWordId, queryWord));
            Assert.IsTrue(wordQuery.Update(queryWordId, queryWord));
        }

        [TestMethod]
        [DataRow("testValue", "testV")]
        public void SearchSingle(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord);
            Assert.AreEqual(queryWord, wordQuery.SearchSingle(searchWord).ToString());

        }
        [TestMethod]
        [DataRow("testValue", "testV")]
        public void SearchSingle_Should_Get_Exception(string queryWord, string searchWord)
        {
            string queryWordId = "testId";
            string queryWordId2 = "testId2";
            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord);
            wordQuery.Add(queryWordId2, queryWord);

            Assert.ThrowsException<System.InvalidOperationException>(() => wordQuery.SearchSingle(searchWord));

        }

        [TestMethod]
        [DataRow("testValue", "testV")]
        public void SearchSingleOrDefault(string queryWord, string searchWord)
        {
            string queryWordId = "testId";
            string queryWordId2 = "testId2";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            string defValue = "defualt";
            Assert.AreEqual(defValue, wordQuery.SearchSingleOrDefault(searchWord, defValue).ToString());

            wordQuery.Add(queryWordId, queryWord);
            wordQuery.Add(queryWordId2, queryWord);
            Assert.AreEqual(defValue, wordQuery.SearchSingleOrDefault(searchWord, defValue).ToString());
        }

        [TestMethod]
        [DataRow("testValue", "testV")]
        public void MultipleQueryableWord(string queryWord, string searchWord)
        {
            string queryWordId = "testId";
            string queryWordId2 = "testId2";
            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord);
            wordQuery.Add(queryWordId2, queryWord);

            var results = wordQuery.Search(searchWord).AsString().ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(queryWord, results[0]);
            Assert.AreEqual(queryWord, results[1]);

        }
        [TestMethod]
        [DataRow("testValue", "testV", "testData")]
        public void QueryableWord_With_Data(string queryWord, string searchWord, string data)
        {
            string queryWordId = "testId";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord, data);

            Assert.AreEqual(data, wordQuery.SearchSingle(searchWord).ToString());
        }

        [TestMethod]
        [DataRow("testValue", "testV", "testData")]
        public void QueryableWord_AddUpdate_Data(string queryWord, string searchWord, string data)
        {
            string queryWordId = "testId";
            string dataSecond = "testData2";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord, data);
            wordQuery.Add(queryWordId, queryWord, dataSecond);

            Assert.AreEqual(dataSecond, wordQuery.SearchSingle(searchWord).ToString());
        }

        [TestMethod]
        [DataRow("testValue", "testV", "testData")]
        public void QueryableWord_Update_Data(string queryWord, string searchWord, string data)
        {
            string queryWordId = "testId";
            string dataSecond = "testData2";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord, data);
            wordQuery.Update(queryWordId, queryWord, Encoding.UTF8.GetBytes(dataSecond));

            Assert.AreEqual(dataSecond, wordQuery.SearchSingle(searchWord).ToString());
        }
        [TestMethod]
        [DataRow("testValue", "testV", "testData")]
        public void QueryableWord_SerializedData(string queryWord, string searchWord, string data)
        {
            string queryWordId = "testId";
            TestObject dataObject = new TestObject(data);

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.Serializer = new TestJsonSerializer();

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.AddObject(queryWordId, queryWord, dataObject);

            var value = wordQuery.SearchSingle<TestObject>(searchWord);
            Assert.AreEqual(dataObject.aProperty, value.aProperty);

        }
        [TestMethod]
        [DataRow("testValue", "testV", "testData")]
        public void QueryableWord_AddRemove(string queryWord, string searchWord, string data)
        {
            string queryWordId = "testId";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord);

            Assert.AreEqual(queryWord, wordQuery.SearchSingle(searchWord).ToString());

            wordQuery.Remove(queryWordId);

            Assert.AreEqual(0, wordQuery.Search(searchWord).Count());

        }

    }
}
