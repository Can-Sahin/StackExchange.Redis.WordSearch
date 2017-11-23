# StackExchange.Redis.WordSearch

AutoComplete, Word Search extension for [StackExchange.Redis] implemented with internal Redis Data Structures.
Extra features:
1. Raw/serialized data attachment to a searchable word.
2. Popularity Ranking 

# Requirements
 1. `.NET Standards 1.6`
 2. `StackExchange.Redis 1.2.6`

# Installation
`PM> Install-Package StackExchange.Redis.WordSearch`

or

`$ dotnet add package StackExchange.Redis.WordSearch`

**See the [docs] for usage and examples**

# Basic Usage
For basic AutoCompletion:
```csharp
//  IDatabase database = ConnectionMultiplexer.GetDatabase();
    IRedisWordSearch redisSearch = new RedisWordSearch(database); //Use IRedisWordSearch to see the xml documentation
    redisSearch.Add("IdOfWatson", "EmmaWatson");
    redisSearch.Add("IdOfStone", "EmmaStone");

    List<string> results = redisSearch.Search("Emm").AsStringList();
    // ["EmmaWatson", "EmmaStone"]
    // Also supports "mma" style searches
```
Ranking Popular Searches:
```csharp
// IDatabase database = ConnectionMultiplexer.GetDatabase();
    RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
    config.RankingProvider = new CurrentlyPopularRanking(ANCHOR_EPOCH, 24); //24 hours half life

    IRedisWordSearch redisSearch = new RedisWordSearch(Database, config);
    redisSearch.Add("IdOfWatson", "EmmaWatson");
    redisSearch.Add("IdOfStone", "EmmaStone");

    // Typically when application-side clicks/votes/interacts to a search result
    redisSearch.BoostInRanking("IdOfStone");
    redisSearch.BoostInRanking("IdOfWatson");
    redisSearch.BoostInRanking("IdOfWatson");
    List<string> results = redisSearch.Search("Emma");
    // ["EmmaWatson", "EmmaStone"] EmmaWatson is ordered first because she is twice as popular

    var words = redisSearch.TopRankedSearches().AsStringList();
    // ["EmmaWatson", "EmmaStone"] EmmaWatson is ordered first because she is twice as popular
```
See the [docs] for detailed `Ranking` explanation

Serialized Data:
```csharp
// IDatabase database = ConnectionMultiplexer.GetDatabase();
    RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
    config.Serializer = new MyJsonSerializer();

    IRedisWordSearch wordSearch = new RedisWordSearch(database,config);
    wordSearch.Add("IdOfWatson", "EmmaWatson", new PartnerOfEmma("HarryPotter"));
    wordSearch.Add("IdOfStone", "EmmaStone", new PartnerOfEmma("PeterParker"));

    List<PartnerOfEmma> results = wordSearch.Search<PartnerOfEmma>("Emma").AsStringList();
```

Use `RedisWordSearchConfiguration` to:
- Enable Popularity Ranking (default half life decay rate algorithm is implemented)
- Limit min/max search length
- Put Redis Key Prefix
- Enable Case Insensivity
- Specify searching method (autocomplete sequentially or search mixed ("mma" for searching "EmmaWatson")

**For details:  [docs]**

# License
MIT

[StackExchange.Redis]: <https://github.com/StackExchange/StackExchange.Redis>
[docs]: <https://github.com/Can-Sahin/StackExchange.Redis.WordSearch/tree/master/Docs>
