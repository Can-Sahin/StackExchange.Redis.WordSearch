using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StackExchange.Redis.WordSearch;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace StackExchange.Redis.WordSearch.Test
{
    [TestClass]
    public class AddSearchTests : BaseTest
    {
        [TestMethod]
        [DataRow("testValue")]
        public void AddWord(string queryWord)
        {
            string queryWordId = "testId";

            RedisWordSearch wordSearch = new RedisWordSearch(Database);
            Assert.IsTrue(wordSearch.Add(queryWordId, queryWord));
        }
        [TestMethod]
        [DataRow("testValue")]
        public void UpdateWord(string queryWord)
        {
            string queryWordId = "testId";

            RedisWordSearch wordSearch = new RedisWordSearch(Database);
            Assert.IsTrue(wordSearch.Add(queryWordId, queryWord));
            Assert.IsTrue(wordSearch.Update(queryWordId, queryWord));
        }

        [TestMethod]
        [DataRow("testValue", "testV")]
        public void SearchSingle(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordSearch wordSearch = new RedisWordSearch(Database);
            wordSearch.Add(queryWordId, queryWord);
            Assert.AreEqual(queryWord, wordSearch.SearchSingle(searchWord).ToString());

        }
        [TestMethod]
        [DataRow("testValue", "testV")]
        public void SearchSingle_Should_Get_Exception(string queryWord, string searchWord)
        {
            string queryWordId = "testId";
            string queryWordId2 = "testId2";
            RedisWordSearch wordSearch = new RedisWordSearch(Database);
            wordSearch.Add(queryWordId, queryWord);
            wordSearch.Add(queryWordId2, queryWord);

            Assert.ThrowsException<System.InvalidOperationException>(() => wordSearch.SearchSingle(searchWord));

        }

        [TestMethod]
        [DataRow("testValue", "testV")]
        public void SearchSingleOrDefault(string queryWord, string searchWord)
        {
            string queryWordId = "testId";
            string queryWordId2 = "testId2";

            RedisWordSearch wordSearch = new RedisWordSearch(Database);
            string defValue = "defualt";
            Assert.AreEqual(defValue, wordSearch.SearchSingleOrDefault(searchWord, defValue).ToString());

            wordSearch.Add(queryWordId, queryWord);
            wordSearch.Add(queryWordId2, queryWord);
            Assert.AreEqual(defValue, wordSearch.SearchSingleOrDefault(searchWord, defValue).ToString());
        }

        [TestMethod]
        [DataRow("testValue", "testV")]
        public void MultipleSearchableWord(string queryWord, string searchWord)
        {
            string queryWordId = "testId";
            string queryWordId2 = "testId2";
            RedisWordSearch wordSearch = new RedisWordSearch(Database);
            wordSearch.Add(queryWordId, queryWord);
            wordSearch.Add(queryWordId2, queryWord);

            var results = wordSearch.Search(searchWord).AsString().ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(queryWord, results[0]);
            Assert.AreEqual(queryWord, results[1]);

        }
        [TestMethod]
        [DataRow("testValue", "testV", "testData")]
        public void SearchableWord_With_Data(string queryWord, string searchWord, string data)
        {
            string queryWordId = "testId";

            RedisWordSearch wordSearch = new RedisWordSearch(Database);
            wordSearch.Add(queryWordId, queryWord, data);

            Assert.AreEqual(data, wordSearch.SearchSingle(searchWord).ToString());
        }

        [TestMethod]
        [DataRow("testValue", "testV", "testData")]
        public void SearchableWord_AddUpdate_Data(string queryWord, string searchWord, string data)
        {
            string queryWordId = "testId";
            string dataSecond = "testData2";

            RedisWordSearch wordSearch = new RedisWordSearch(Database);
            wordSearch.Add(queryWordId, queryWord, data);
            wordSearch.Add(queryWordId, queryWord, dataSecond);

            Assert.AreEqual(dataSecond, wordSearch.SearchSingle(searchWord).ToString());
        }

        [TestMethod]
        [DataRow("testValue", "testV", "testData")]
        public void SearchableWord_Update_Data(string queryWord, string searchWord, string data)
        {
            string queryWordId = "testId";
            string dataSecond = "testData2";

            RedisWordSearch wordSearch = new RedisWordSearch(Database);
            wordSearch.Add(queryWordId, queryWord, data);
            wordSearch.Update(queryWordId, queryWord, Encoding.UTF8.GetBytes(dataSecond));

            Assert.AreEqual(dataSecond, wordSearch.SearchSingle(searchWord).ToString());
        }
        [TestMethod]
        [DataRow("testValue", "testV", "testData")]
        public void SearchableWord_SerializedData(string queryWord, string searchWord, string data)
        {
            string queryWordId = "testId";
            TestObject dataObject = new TestObject(data);

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
            config.Serializer = new TestJsonSerializer();

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.AddObject(queryWordId, queryWord, dataObject);

            var value = wordSearch.SearchSingle<TestObject>(searchWord);
            Assert.AreEqual(dataObject.aProperty, value.aProperty);

        }
        [TestMethod]
        [DataRow("testValue", "testV", "testData")]
        public void SearchableWord_AddRemove(string queryWord, string searchWord, string data)
        {
            string queryWordId = "testId";

            RedisWordSearch wordSearch = new RedisWordSearch(Database);
            wordSearch.Add(queryWordId, queryWord);

            Assert.AreEqual(queryWord, wordSearch.SearchSingle(searchWord).ToString());

            wordSearch.Remove(queryWordId);

            Assert.AreEqual(0, wordSearch.Search(searchWord).Count());

        }

    }
}
