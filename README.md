# w3champions-statistic-service

The statisticservice is the Backend for the https://github.com/w3champions/w3champions-ui Project. Among the data for
the website it also provides the chat backend that is used from the ingame client and the clan functions.

## Setup
You will need a mongodb to run the service. If you do not have a local mongo container you can use the mongo db
connectionstring "mongodb://176.28.16.249:3513".

If you have your own MongoDb, you need to run the service with a Env Variable Called "MONGO_CONNECTION_STRING" and
set it to the corresponding connection string. You can also just replace the line in the Startup.cs with the needed
connection string (Line 57).

CAUTION:
When running locally, the readmodel handling is turned off, unless you set the corresponding env variable. But only
do that, if you know what you are doing. You can end up overwriting data from prod/test, depending where you are
connected. So always make sure the handlers are turned off, or you have breakpoints set before you save any data.
(more to readmodels below).

## Read Model Handling
The readmodels for the DB are being generated from the event that the matchmaking service generates. After a match,
the matchmaking service pushes an event to the collection "MatchFinishedEvent". For every readmodel that we have, we
read the latest event and transform it into the model for the website. Like that we also generate statistics that are
bigger and would be too costly to calculate every time we enter the page (like the hero vs her stats for example).
This also decouples the matchmaking service from the stuff that we need to show on the website.

CAUTION:
When running locally, the readmodel handling is turned off, unless you set the corresponding env variable. But only
do that, if you know what you are doing. You can end up overwriting data from prod/test, depending where you are
connected. So always make sure the handlers are turned off, or you have breakpoints set before you save any data.

### Versioned Handlers
Those are the default Handlers and you can implement one like so
```
public class PlayerWinrateHandler : IReadModelHandler
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerWinrateHandler(
        IPlayerRepository playerRepository
        )
    {
        _playerRepository = playerRepository;
    }

    public async Task Update(MatchFinishedEvent nextEvent) // this is the method you need to implement
    {
        var playerMMrChanges = nextEvent.match.players;
        var winrateTasks = playerMMrChanges.Select(async p => await LoadAndApply(p, nextEvent.match.season));
        var newWinrates = (await Task.WhenAll(winrateTasks)).ToList();
        await _playerRepository.UpsertWins(newWinrates);
    }
}
```

As you can see you need to make the transformation and save the readmodel in our database. The convention always was,
 that we call the handler like the readmodel. In that case `PlayerWinrateHandler` and `PlayerWinrate`, so it is
 easier to find the collection to the handler.
 Also the handling framework saves the version of the last save event, when the function returns. (See
 ReadModelHandler<T> class for details). The version is saved in the collection `HandlerVersions`. So it makes sens
 to take a look at the versions from time to time. If they differ and do not move, it is likely that a handler is
 throwing exceptions and is stuck. The retry time is every 5 seconds.

 ### Unversioned Handlers
 We also have unversioned handlers for stuff that is not tight to an event. For example the Rankings are also pushed
 to the database after a match and we load all of them in a handler, transform all ranks and then save all of them to
  our readmodel db. That is usually not needed, but the interface for that is `IAsyncUpdatable`. This function is
  called every 5 seconds aswell and you can do whatever you want in there periodically. Like a chron job for poor
  people ;)

## Import Export Mongodata
You need mongodb installed to have the mentioned toole here.

Export with (use the complete connection string here ofc)
```
mongodump --uri="mongodb://w3champions....."
```
creates a dump folder with the data.

Import to Test DB
```
mongorestore --uri="mongodb://176.28.16.249:3513" dump/
```

This takes quite some time, an hour or so btw.