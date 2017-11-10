using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis.WordQuery;

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
        public void Add_QueryableWord_Dont_Update_Data()
        {
            string queryWord = "testValue";
            string queryWordId = "testId";
            string dataFirst = "testData";
            string dataSecond = "testData2";

            FlushDB();

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord, dataFirst, false);

            wordQuery.Add(queryWordId, queryWord, dataSecond);


            string value = Database.HashGet(QueryableDataHashKey, queryWordId);
            Assert.AreEqual(dataSecond, value);
        }
    }
}
