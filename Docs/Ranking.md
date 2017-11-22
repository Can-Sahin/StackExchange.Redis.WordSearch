## Ranking

**How:** Provide ranking algorithm to the RedisWordSearchConfiguration 

**Notes:**

1. All search results are ordered by their scores in descending order
2. You can provide your own algorithm sticking to the interfaces
3. Currently only Relative-Decay-Rate algorithms are supported where scores of the searchable items only increases as the time passes. Ranking algorithm is responsible for providing how much the score will be implemented at any given time. This mean precision overflows will HAPPEN eventually!


### `CurrentlyPopularRanking`

**Note:**
Algorithm is from: http://qwerjk.com/posts/surfacing-interesting-content/

**How it works:** It everytime increments the score of item by a certain value depending on the half life and anchor epoch. If half life time is 24 hours, it means 1 interaction now will have the same score as 2 interactions happened 24 hours ago. Hence the name 'half life'. After a while all scores will accumulate and redis sorted set scores limits will be exceeded. Sorted set can represent scores upto a certain limit of double values. The shorter half life is the shorter it takes to reach the limit. Then you have migrate all the keys as it is explained in the link. Pick 24 hours and you might not need to migrate for years. Pick 10 minutes and you will reach the point in couple days. 

So, pick your `PopularRankingConstEPOCH` as so that inital score will be very small

`PopularRankingConstEPOCH = CONST_UNIX_TIME_BEFORE_DEPLOYMENT+ (1074 * HalfLifeInSeconds)`

This will initally give you double.MinValue (approx). Where CONST_UNIX_TIME_BEFORE_DEPLOYMENT is the epoch that needs to be hard coded before running/deploying. It will be the anchor

This algorithm will aproximatly will run for `(1074 + 1023) * Half_Life_inHours` hours before needing a migration.

Be aware of when you need to migrate.

**Example `CurrentlyPopularRanking` usage** :
```csharp
    const long deploymentTime = 1511381314;
    const long PopularRankingConstEPOCH = deploymentTime + (1074 * halfLifeSeconds) ;
    static popularRanking = new CurrentlyPopularRanking(PopularRankingConstEPOCH, Half_Life);

    RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
    
    double Half_Life = 24 // Hours
    int halfLifeSeconds = Half_Life * 60 * 60;

    // Assign the same instance with same constEpoch 
    // DONT CHANGE THE PopularRankingEPOCH throughout the code
    config.RankingProvider = popularRanking;


    RedisWordSearch redisSearch = new RedisWordSearch(Database, config);
    redisSearch.Add("IdOfWatson", "EmmaWatson");
    redisSearch.Add("IdOfStone", "EmmaStone");

    // Typically when application-side clicks/votes/interacts to a search result
    redisSearch.BoostInRanking("IdOfStone");
    redisSearch.BoostInRanking("IdOfWatson");
    double? newScore = redisSearch.BoostInRanking("IdOfWatson");
    if(!newScore.HasValue) // Either redis transaction is unsuccessfull or its time to migrate!

    List<string> results = redisSearch.Search("Emma");
    // ["EmmaWatson", "EmmaStone"] EmmaWatson is ordered first because she is twice as popular

    var words = redisSearch.TopRankedSearches().AsStringList();
    // ["EmmaWatson", "EmmaStone"] EmmaWatson is ordered first because she is twice as popular
```

