# StackExchange.Redis.WordQuery

AutoComplete, Word Search extension for [StackExchange.Redis] implemented with internal Redis Data Structures. Supports raw/serialized data attachment to a queryable word.
Currently this project provides very basic features and implementation. I will try to improve in time.
# Installation
`PM> Install-Package StackExchange.Redis.WordQuery`

# Usage

For AutoCompletion:
```csharp
	// IDatabase database = ConnectionMultiplexer.GetDatabase();
    RedisWordQuery wordQuery = new RedisWordQuery(database);
    wordQuery.Add("UserID", "EmmaWatson");
    wordQuery.Add("UserID2", "EmmaStone");

    List<string> results = wordQuery.Search("Emm");
    // ["EmmaWatson", "EmmaStone"]
    // Also supports "mma" style searches
```

For searching rich data:

```csharp
	// IDatabase database = ConnectionMultiplexer.GetDatabase();
    RedisWordQuery wordQuery = new RedisWordQuery(database);
    wordQuery.Add("UserID", "EmmaWatson", "PhoneNumberOfEmmaWatson");
    wordQuery.Add("UserID2", "EmmaStone", "PhoneNumberOfEmmaStone");

    List<string> results = wordQuery.Search("Emm");
    // ["PhoneNumberOfEmmaWatson", "PhoneNumberOfEmmaStone"]
```
Serialized Data:
```csharp
	// IDatabase database = ConnectionMultiplexer.GetDatabase();
    RedisWordQueryConfiguration config = RedisWordQueryConfiguration.defaultConfig;
    config.Serializer = new MyJsonSerializer();

    RedisWordQuery wordQuery = new RedisWordQuery(database);
    wordQuery.Add("UserID", "EmmaWatson", new GiftForEmma());
    wordQuery.Add("UserID2", "EmmaStone", new GiftForEmma());

    List<GiftForEmma> results = wordQuery.Search<GiftForEmma>("Emma");
```
Use `RedisWordQueryConfiguration` to :
- Limit min/max search length
- Put Redis Prefix
- Case Insensivity
- Specify search method (autocomplete sequentially or search mixed ("mma" for searching "EmmaWatson")
	
# ToDo
- Hot/Trending Search Ranking

Currently this project provides very basic features and implementation. I will try to improve in time.

# License
MIT

[StackExchange.Redis]: <https://github.com/StackExchange/StackExchange.Redis>
