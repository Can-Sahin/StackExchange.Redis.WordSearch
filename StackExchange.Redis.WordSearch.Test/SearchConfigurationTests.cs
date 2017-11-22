using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis.WordSearch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace StackExchange.Redis.WordSearch.Test
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

            RedisWordSearch wordSearch = new RedisWordSearch(Database);
            wordSearch.Add(queryWordId, queryWord);

            var results = wordSearch.Search(queryWord).AsStringList();
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

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
            config.WordIndexingMethod = WordIndexing.SequentialCombination;

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);

            var results = wordSearch.Search(searchWord).AsStringList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(queryWord, results[0]);
        }

        [TestMethod]
        [DataRow("testValue", "testV")]
        public void CaseSensitiveConfiguration(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
            config.IsCaseSensitive = true;

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);

            Assert.AreEqual(queryWord, wordSearch.SearchSingle(searchWord).ToString());
            Assert.AreEqual("default", wordSearch.SearchSingleOrDefault(searchWord.ToLower(), "default").ToString());

            wordSearch.Add(queryWordId.ToLower(), queryWord.ToLower());
            Assert.AreEqual(queryWord.ToLower(), wordSearch.SearchSingle(searchWord.ToLower()).ToString());

        }
        [TestMethod]
        [DataRow("testValue", "testV")]
        public void CaseInSensitiveConfiguration(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
            config.IsCaseSensitive = false;

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);

            Assert.AreEqual(queryWord, wordSearch.SearchSingle(searchWord).ToString());
            Assert.AreEqual(queryWord, wordSearch.SearchSingleOrDefault(searchWord.ToLower(), "default").ToString());

            wordSearch.Add(queryWordId.ToLower(), queryWord.ToLower(), "someData");
            Assert.AreEqual("someData", wordSearch.SearchSingle(searchWord).ToString());
        }
        [TestMethod]
        [DataRow("testValue", "te")]
        public void MinQueryLengthLimit_EmptyResults(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
            config.MinQueryLength = 3;

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);

            Assert.AreEqual(0, wordSearch.Search(searchWord).Count());
        }

        [TestMethod]
        [DataRow("testValue", "testVa")]
        public void MaxQueryLengthLimit_EmptyResults(string queryWord, string searchWord)
        {
            string queryWordId = "testId";

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
            config.MaxQueryLength = 5;

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);

            Assert.AreEqual(0, wordSearch.Search(searchWord).Count());
        }
        [TestMethod]
        public void Min_MaxQueryLengthLimit_Sequential()
        {
            string queryWord = "testValue";
            string searcWordCorrect = "test";
            string searchWordEmpty = "te";
            string searchWordEmpty2 = "testVal";

            string queryWordId = "testId";

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
            config.MinQueryLength = 3;
            config.MaxQueryLength = 5;

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);

            Assert.AreEqual(1, wordSearch.Search(searcWordCorrect).Count());

            Assert.AreEqual(0, wordSearch.Search(searchWordEmpty).Count());

            Assert.AreEqual(0, wordSearch.Search(searchWordEmpty2).Count());

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

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
            config.MaxQueryLength = 5;
            config.MinQueryLength = 3;
            config.WordIndexingMethod = WordIndexing.SequentialCombination;

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);

            Assert.AreEqual(1, wordSearch.Search(searcWordCorrect).Count());
            Assert.AreEqual(1, wordSearch.Search(searcWordCorrect2).Count());
            Assert.AreEqual(1, wordSearch.Search(searcWordCorrect3).Count());
            Assert.AreEqual(1, wordSearch.Search(searcWordCorrect4).Count());
            Assert.AreEqual(0, wordSearch.Search(searchWordEmpty).Count());
            Assert.AreEqual(0, wordSearch.Search(searchWordEmpty2).Count());

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

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);
            wordSearch.Add(queryWordId2, queryWord2);
            wordSearch.Add(queryWordId3, queryWord3);

            Assert.AreEqual(3, wordSearch.Search(searchWord).Count());
            Assert.AreEqual(2, wordSearch.Search(searchWord, limit: 2).Count());
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

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);
            wordSearch.Add(queryWordId2, queryWord2);
            wordSearch.Add(queryWordId3, queryWord3);

            Func<RedisValue, bool> filterFunc = value =>
            {
                if (value.Equals(queryWord))
                {
                    return false;
                }
                return true;
            };

            var results = wordSearch.Search(searchWord, filterFunc: filterFunc).AsStringList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(queryWord, results[0]);

        }

        [TestMethod]
        public void IncrementRanking()
        {
            string queryWordId = "testId";
            string queryWord = "testValue";

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
            config.RankingProvider = new CurrentlyPopularRanking(AppSettings.RANKING_EPOCH, 1);

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);

            wordSearch.BoostInRanking(queryWordId);
            Assert.AreNotEqual(0, wordSearch.CurrentScore(queryWordId));
        }

        [TestMethod]
        public void TopRankings()
        {
            string queryWordId = "testId";
            string queryWordId2 = "testId2";

            string queryWord = "testValue";
            string queryWord2 = "testValue2";

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
            config.RankingProvider = new CurrentlyPopularRanking(AppSettings.RANKING_EPOCH, 1);

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);
            wordSearch.BoostInRanking(queryWordId);

            wordSearch.Add(queryWordId2, queryWord2);
            wordSearch.BoostInRanking(queryWordId2);

            wordSearch.BoostInRanking(queryWordId);

            var words = wordSearch.TopRankedSearches().AsStringList();

            Assert.AreEqual(queryWord, words[0]);
            Assert.AreEqual(queryWord2, words[1]);
        }

        [TestMethod]
        public void IncrementRanking_TestHalfLife()
        {
            string queryWordId = "testId";
            string queryWordId2 = "testId2";

            string queryWord = "testValue";
            double secondsToWait = 1;

            RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
            long minAgo = DateTimeOffset.UtcNow.AddSeconds(-10).ToUnixTimeSeconds();

            double halfLife = secondsToWait / 3600;
            config.RankingProvider = new CurrentlyPopularRanking(minAgo, halfLife);

            RedisWordSearch wordSearch = new RedisWordSearch(Database, config);
            wordSearch.Add(queryWordId, queryWord);
            wordSearch.Add(queryWordId2, queryWord);

            wordSearch.BoostInRanking(queryWordId);
            wordSearch.BoostInRanking(queryWordId);

            Thread.Sleep((int)secondsToWait * 1000);

            wordSearch.BoostInRanking(queryWordId2);

            double? score1 = wordSearch.CurrentScore(queryWordId);
            double? score2 = wordSearch.CurrentScore(queryWordId2);

            Assert.IsTrue( (score2 == score1) || (score2 == score1 * 2)) ;
        }

    }
}
