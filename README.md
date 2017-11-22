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

# Usage
For basic AutoCompletion:
```csharp
//  IDatabase database = ConnectionMultiplexer.GetDatabase();
    RedisWordSearch redisSearch = new RedisWordSearch(database);
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
    double Half_Life = 24 // Hours
    config.RankingProvider = new CurrentlyPopularRanking(AppSettings.RANKING_EPOCH, Half_Life);

    RedisWordSearch redisSearch = new RedisWordSearch(Database, config);
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
See the [docs] for detailed explanation

Serialized Data:
```csharp
// IDatabase database = ConnectionMultiplexer.GetDatabase();
    RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;
    config.Serializer = new MyJsonSerializer();

    RedisWordSearch wordSearch = new RedisWordSearch(database,config);
    wordSearch.Add("IdOfWatson", "EmmaWatson", new PartnerOfEmma("HarryPotter"));
    wordSearch.Add("IdOfStone", "EmmaStone", new PartnerOfEmma("PeterParker"));

    List<PartnerOfEmma> results = wordSearch.Search<PartnerOfEmma>("Emma").AsStringList();
```
Check [docs] for more usage examples

Use `RedisWordSearchConfiguration` to:
- Enable Popularity Ranking (default half life decay rate algorithm is implemented)
- Limit min/max search length
- Put Redis Key Prefix
- Enable Case Insensivity
- Specify searching method (autocomplete sequentially or search mixed ("mma" for searching "EmmaWatson")

# License
MIT

[StackExchange.Redis]: <https://github.com/StackExchange/StackExchange.Redis>
[docs]: <https://github.com/Can-Sahin/StackExchange.Redis.WordSearch/tree/master/Docs>
