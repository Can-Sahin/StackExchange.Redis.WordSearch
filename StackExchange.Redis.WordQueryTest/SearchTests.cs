using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis.WordQuery;
using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordQueryTest
{
    [TestClass]
    public class SearchTests:BaseTest
    {
        [TestMethod]
        [DataRow("testValue","t")]
        [DataRow("testValue", "te")]
        [DataRow("testValue", "tes")]
        [DataRow("testValue", "tes")]
        [DataRow("testValue", "test")]
        [DataRow("testValue", "testV")]
        [DataRow("testValue", "testVa")]
        [DataRow("testValue", "testVal")]
        [DataRow("testValue", "testValu")]
        [DataRow("testValue", "testValue")]
        public void QueryWord_Sequential(string originalWord, string queryWord)
        {

            string originalId = "testId";

            FlushDB();
            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(originalId, originalWord);

            string value = Database.HashGet(QueryableHashKey, originalId);
            Assert.AreEqual(originalWord, value);

            var results = wordQuery.Search(queryWord);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(originalWord, results[0].ToString());

        }
        [TestMethod]
        [DataRow("testValue", "t")]
        [DataRow("testValue", "te")]
        [DataRow("testValue", "tes")]
        [DataRow("testValue", "tes")]
        [DataRow("testValue", "test")]
        [DataRow("testValue", "testV")]
        [DataRow("testValue", "testVa")]
        [DataRow("testValue", "testVal")]
        [DataRow("testValue", "testValu")]
        [DataRow("testValue", "testValue")]
        [DataRow("testValue", "est")]
        [DataRow("testValue", "estV")]
        [DataRow("testValue", "estVal")]
        [DataRow("testValue", "estValu")]
        [DataRow("testValue", "estValue")]
        [DataRow("testValue", "stValue")]
        [DataRow("testValue", "tValue")]
        [DataRow("testValue", "Value")]
        public void QueryWord_SequentialCombination(string originalWord, string queryWord)
        {
            string originalId = "testId";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.WordIndexingMethod = WordIndexing.SequentialCombination;

            RedisWordQuery wordQuery = new RedisWordQuery(Database,config);
            wordQuery.Add(originalId, originalWord);

            string value = Database.HashGet(QueryableHashKey, originalId);
            Assert.AreEqual(originalWord, value);

            var results = wordQuery.Search(queryWord);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(originalWord, results[0].ToString());
        }

        [TestMethod]
        public void MinPrefixLimit_EmptyResults()
        {
            FlushDB();
            string originalWord = "testValue";
            string queryWord = "te";

            string originalId = "testId";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.MinPrefixLength = 3;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(originalId, originalWord);

            string value = Database.HashGet(QueryableHashKey, originalId);
            Assert.AreEqual(originalWord, value);

            var results = wordQuery.Search(queryWord);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void MaxPrefixLimit_EmptyResults()
        {
            FlushDB();
            string originalWord = "testValue";
            string queryWord = "testValu";

            string originalId = "testId";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.MaxPrefixLength = 5;


            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(originalId, originalWord);

            string value = Database.HashGet(QueryableHashKey, originalId);
            Assert.AreEqual(originalWord, value);

            var results = wordQuery.Search(queryWord);
            Assert.AreEqual(0, results.Count);
        }
        [TestMethod]
        public void Min_MaxPrefixLimit_Sequential()
        {
            FlushDB();
            string originalWord = "testValue";
            string queryWordCorrect = "test";
            string queryWordEmpty = "te";
            string queryWordEmpty2 = "testVal";

            string originalId = "testId";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.MaxPrefixLength = 5;
            config.MinPrefixLength = 3;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(originalId, originalWord);

            string value = Database.HashGet(QueryableHashKey, originalId);
            Assert.AreEqual(value, originalWord);

            var results = wordQuery.Search(queryWordCorrect);
            Assert.AreEqual(1, results.Count);

            var results2 = wordQuery.Search(queryWordEmpty);
            Assert.AreEqual(0, results2.Count);

            var results3 = wordQuery.Search(queryWordEmpty2);
            Assert.AreEqual(0, results3.Count);
        }
        [TestMethod]
        public void Min_MaxPrefixLimit_SequentialCombination()
        {
            FlushDB();
            string originalWord = "testValue";
            string queryWordCorrect = "test";
            string queryWordCorrect2 = "estV";
            string queryWordCorrect3 = "tVal";
            string queryWordCorrect4 = "Valu";


            string queryWordEmpty = "te";
            string queryWordEmpty2 = "testVal";

            string originalId = "testId";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.MaxPrefixLength = 5;
            config.MinPrefixLength = 3;
            config.WordIndexingMethod = WordIndexing.SequentialCombination;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(originalId, originalWord);

            string value = Database.HashGet(QueryableHashKey, originalId);
            Assert.AreEqual(value, originalWord);

            var results = wordQuery.Search(queryWordCorrect);
            Assert.AreEqual(1, results.Count);

            var results2 = wordQuery.Search(queryWordCorrect2);
            Assert.AreEqual(1, results2.Count);

            var results3 = wordQuery.Search(queryWordCorrect3);
            Assert.AreEqual(1, results3.Count);

            var results4 = wordQuery.Search(queryWordCorrect4);
            Assert.AreEqual(1, results4.Count);

            var results5 = wordQuery.Search(queryWordEmpty);
            Assert.AreEqual(0, results5.Count);

            var results6 = wordQuery.Search(queryWordEmpty2);
            Assert.AreEqual(0, results6.Count);
        }
        [TestMethod]
        public void QueryResultLimit()
        {
            FlushDB();
            string originalWord = "testValue";
            string originalWord2 = "testValue2";
            string originalWord3 = "testValue3";
            string queryWord = "test";

            string originalId = "testId";
            string originalId2 = "testId2";
            string originalId3 = "testId3";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(originalId, originalWord);
            wordQuery.Add(originalId2, originalWord2);
            wordQuery.Add(originalId3, originalWord3);

            Assert.AreEqual(originalWord, (string)Database.HashGet(QueryableHashKey, originalId));
            Assert.AreEqual(originalWord2, (string)Database.HashGet(QueryableHashKey, originalId2));
            Assert.AreEqual(originalWord3, (string)Database.HashGet(QueryableHashKey, originalId3));

            var results = wordQuery.Search(queryWord,limit:2);
            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void QueryResultFilter()
        {
            FlushDB();
            string originalWord = "testValue";
            string originalWord2 = "testValue2";
            string originalWord3 = "testValue3";
            string queryWord = "test";

            string originalId = "testId";
            string originalId2 = "testId2";
            string originalId3 = "testId3";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(originalId, originalWord);
            wordQuery.Add(originalId2, originalWord2);
            wordQuery.Add(originalId3, originalWord3);

            Assert.AreEqual(originalWord, (string)Database.HashGet(QueryableHashKey, originalId));
            Assert.AreEqual(originalWord2, (string)Database.HashGet(QueryableHashKey, originalId2));
            Assert.AreEqual(originalWord3, (string)Database.HashGet(QueryableHashKey, originalId3));

            Func<RedisValue,bool> filterFunc =  value =>
            {
                if (value.Equals(originalWord))
                {
                    return false;
                }
                return true;
             };
            
            var results = wordQuery.Search(queryWord,filterFunc: filterFunc);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(originalWord, results[0].ToString());

        }
    }
}
