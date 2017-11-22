## Configuration

Currently default values: 
```charp
    public static RedisWordSearchConfiguration defaultConfig = new RedisWordSearchConfiguration
    {
        MinSearchLength = 1,
        MaxSearchLength = -1, // No limit
        IsCaseSensitive = true,
        KeyNameConfiguration = new DefaultRedisKeyNameConfiguration(),
        WordIndexingMethod = WordIndexing.SequentialOnly
    };
    public class DefaultRedisKeyNameConfiguration : IRedisKeyNameConfiguration
    {
        public string Seperator => "::";
        public string ContainerPrefix => "WS";
        public string SearchableItemsSuffix => "SearchableItems";
        public string SearchableItemsDataSuffix => "SearchableItemsData";
        public string SearchableSuffix => "S";
        public string SearchableItemsRankingSuffix => "SearchableItemsDataRanking";
    }
```
## Example Cases

```csharp
    // Set each config if you want to change that particular settings
    // Then create RedisWordSearch with that configuration. Otherwise DefaultRedisKeyNameConfiguration is used.

    RedisWordSearchConfiguration config = RedisWordSearchConfiguration.defaultConfig;

    config.MinSearchLength = 3; 
    config.MaxSearchLength = 12; // -1 for no limit

    config.IsCaseSensitive = False; // Converts all searches and storages to lowerCase. "Emma" will be stored as "emma"

    //If say "EmmaWatson" is added previously

    config.WordIndexingMethod = WordIndexing.SequentialOnly; // Can only index and find when searched like "Emm", "Emma", "EmmaW"...

    config.WordIndexingMethod = WordIndexing.SequentialCombination; // Can index find with mixed substrings like "Emm", "mma", "Watson", "mmaWats" ... 

    config.Serializer = new SomeClassInheritsFromISerializer(); // Provide your serializer for data. Throws exception at deserialization when not set

    config.KeyNameConfiguration = new SomeClassInheritsFromIRedisKeyNameConfiguration(); // Provide your own Redis key prefixes for storing searchable items. 

    //Finally 
    // IDatabase database = ConnectionMultiplexer.GetDatabase();
    RedisWordSearch redisSearch = new RedisWordSearch(Database, config);
    .
    .
    .

```

