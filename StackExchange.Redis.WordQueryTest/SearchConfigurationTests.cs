using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis.WordQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace StackExchange.Redis.WordQueryTest
{
    [TestClass]
    public class SearchTests : BaseTest
    {
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
        public void Search_Sequential(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordQuery wordQuery = new RedisWordQuery(Database);
            wordQuery.Add(queryWordId, queryWord);

            var results = wordQuery.Search(queryWord).AsStringList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(queryWord, results[0]);

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
        public void Search_SequentialCombination(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.WordIndexingMethod = WordIndexing.SequentialCombination;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(queryWordId, queryWord);

            var results = wordQuery.Search(searchWord).AsStringList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(queryWord, results[0]);
        }

        [TestMethod]
        [DataRow("testValue", "testV")]
        public void CaseSensitiveConfiguration(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.IsCaseSensitive = true;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(queryWordId, queryWord);

            Assert.AreEqual(queryWord, wordQuery.SearchSingle(searchWord).ToString());
            Assert.AreEqual("default", wordQuery.SearchSingleOrDefault(searchWord.ToLower(), "default").ToString());

            wordQuery.Add(queryWordId.ToLower(), queryWord.ToLower());
            Assert.AreEqual(queryWord.ToLower(), wordQuery.SearchSingle(searchWord.ToLower()).ToString());

        }
        [TestMethod]
        [DataRow("testValue", "testV")]
        public void CaseInSensitiveConfiguration(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.IsCaseSensitive = false;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(queryWordId, queryWord);

            Assert.AreEqual(queryWord, wordQuery.SearchSingle(searchWord).ToString());
            Assert.AreEqual(queryWord, wordQuery.SearchSingleOrDefault(searchWord.ToLower(), "default").ToString());

            wordQuery.Add(queryWordId.ToLower(), queryWord.ToLower(), "someData");
            Assert.AreEqual("someData", wordQuery.SearchSingle(searchWord).ToString());
        }
        [TestMethod]
        [DataRow("testValue", "te")]
        public void MinQueryLengthLimit_EmptyResults(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.MinQueryLength = 3;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(queryWordId, queryWord);

            Assert.AreEqual(0, wordQuery.Search(searchWord).Count());
        }

        [TestMethod]
        [DataRow("testValue", "testVa")]
        public void MaxQueryLengthLimit_EmptyResults(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.MaxQueryLength = 5;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(queryWordId, queryWord);

            Assert.AreEqual(0, wordQuery.Search(searchWord).Count());
        }
        [TestMethod]
        public void Min_MaxQueryLengthLimit_Sequential()
        {
            string queryWord = "testValue";
            string searcWordCorrect = "test";
            string searchWordEmpty = "te";
            string searchWordEmpty2 = "testVal";

            string queryWordId = "testId";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.MinQueryLength = 3;
            config.MaxQueryLength = 5;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(queryWordId, queryWord);

            Assert.AreEqual(1, wordQuery.Search(searcWordCorrect).Count());

            Assert.AreEqual(0, wordQuery.Search(searchWordEmpty).Count());

            Assert.AreEqual(0, wordQuery.Search(searchWordEmpty2).Count());

        }
        [TestMethod]
        public void Min_MaxQueryLengthLimit_SequentialCombination()
        {
            string queryWordId = "testId";
            string queryWord = "testValue";
            string searcWordCorrect = "test";
            string searcWordCorrect2 = "estV";
            string searcWordCorrect3 = "tVal";
            string searcWordCorrect4 = "Valu";
            string searchWordEmpty = "te";
            string searchWordEmpty2 = "testVal";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.MaxQueryLength = 5;
            config.MinQueryLength = 3;
            config.WordIndexingMethod = WordIndexing.SequentialCombination;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(queryWordId, queryWord);

            Assert.AreEqual(1, wordQuery.Search(searcWordCorrect).Count());
            Assert.AreEqual(1, wordQuery.Search(searcWordCorrect2).Count());
            Assert.AreEqual(1, wordQuery.Search(searcWordCorrect3).Count());
            Assert.AreEqual(1, wordQuery.Search(searcWordCorrect4).Count());
            Assert.AreEqual(0, wordQuery.Search(searchWordEmpty).Count());
            Assert.AreEqual(0, wordQuery.Search(searchWordEmpty2).Count());

        }
        [TestMethod]
        public void ResultLimit()
        {
            string queryWord = "testValue";
            string queryWord2 = "testValue2";
            string queryWord3 = "testValue3";
            string searchWord = "test";

            string queryWordId = "testId";
            string queryWordId2 = "testId2";
            string queryWordId3 = "testId3";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(queryWordId, queryWord);
            wordQuery.Add(queryWordId2, queryWord2);
            wordQuery.Add(queryWordId3, queryWord3);

            Assert.AreEqual(3, wordQuery.Search(searchWord).Count());
            Assert.AreEqual(2, wordQuery.Search(searchWord, limit: 2).Count());
        }

        [TestMethod]
        public void SearchResultFilter()
        {
            string queryWord = "testValue";
            string queryWord2 = "testValue2";
            string queryWord3 = "testValue3";
            string searchWord = "test";

            string queryWordId = "testId";
            string queryWordId2 = "testId2";
            string queryWordId3 = "testId3";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(queryWordId, queryWord);
            wordQuery.Add(queryWordId2, queryWord2);
            wordQuery.Add(queryWordId3, queryWord3);

            Func<RedisValue, bool> filterFunc = value =>
            {
                if (value.Equals(queryWord))
                {
                    return false;
                }
                return true;
            };

            var results = wordQuery.Search(searchWord, filterFunc: filterFunc).AsStringList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(queryWord, results[0]);

        }
        [TestMethod]
        public void IncrementRanking()
        {
            string queryWordId = "testId";
            string queryWord = "testValue";
            string searchWord = "testV";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            config.RankingProvider = new PopularStreamRanking(AppSettings.RANKING_EPOCH, 1);

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(queryWordId, queryWord);

            wordQuery.IncrementRanking(queryWordId);
            Assert.AreNotEqual(0, wordQuery.CurrentScore(queryWordId));
        }

        [TestMethod]
        public void IncrementRanking_TestHalfLife()
        {
            string queryWordId = "testId";
            string queryWordId2 = "testId2";

            string queryWord = "testValue";
            string searchWord = "testV";

            RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
            double halfLife = (double)5 / 3600 ; // 3 seconds
            config.RankingProvider = new PopularStreamRanking(AppSettings.RANKING_EPOCH, halfLife);

            RedisWordQuery wordQuery = new RedisWordQuery(Database, config);
            wordQuery.Add(queryWordId, queryWord);
            wordQuery.Add(queryWordId2, queryWord);

            wordQuery.IncrementRanking(queryWordId);
            wordQuery.IncrementRanking(queryWordId);

            Thread.Sleep(3000);
            wordQuery.IncrementRanking(queryWordId2);

            double score1 = wordQuery.CurrentScore(queryWordId);
            double score2 = wordQuery.CurrentScore(queryWordId2);

            Assert.IsTrue(score2 > score1);
        }

    }
}
