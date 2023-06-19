module FunPizzaShop.Query.Projection

open FSharp.Data.Sql
open Serilog
open Akka.Persistence.Query
open Akka.Persistence.Query.Sql
open FSharp.Data.Sql.Common
open Akkling.Streams
open Akka.Streams
open Thoth.Json.Net
open Command.Serialization


[<Literal>]
let resolutionPath = __SOURCE_DIRECTORY__ + @"/libs"

[<Literal>]
let schemaLocation = __SOURCE_DIRECTORY__ + @"/../Server/Database/Schema.sqlite"
#if DEBUG

[<Literal>]
let connectionString =
    @"Data Source=" + __SOURCE_DIRECTORY__ + @"/../Server/Database/FunPizzaShop.db;"

#else

[<Literal>]
let connectionString = @"Data Source=" + @"Database/FunPizzaShop.db;"

#endif

[<Literal>]
let connectionStringReal = @"Data Source=" + @"Database/FunPizzaShop.db;"

type Sql =
    SqlDataProvider<DatabaseProviderTypes.SQLITE, SQLiteLibrary=SQLiteLibrary.MicrosoftDataSqlite, ConnectionString=connectionString, ResolutionPath=resolutionPath,
  //  ContextSchemaPath=schemaLocation,
    CaseSensitivityChange=CaseSensitivityChange.ORIGINAL>

let ctx = Sql.GetDataContext(connectionString)



let inline encoder<'T> =
    Encode.Auto.generateEncoderCached<'T> (caseStrategy = CamelCase, extra = extraThoth)

let inline decoder<'T> =
    Decode.Auto.generateDecoderCached<'T> (caseStrategy = CamelCase, extra = extraThoth)



type DataEvent = UserEvent of string
  

let handleEvent (connectionString: string) (subQueue: ISourceQueue<_>) (envelop: EventEnvelope) =
    failwith "not implemented"


let handleEventWrapper (connectionString: string) (subQueue: ISourceQueue<_>) (envelop: EventEnvelope) =
    try
        handleEvent connectionString subQueue envelop
    with ex ->
        Log.Error(ex, "Error during event handling")
        System.Environment.FailFast("Failfasting Error during event handling")

open Akkling.Streams
open Command.Actor

let readJournal system =
    PersistenceQuery
        .Get(system)
        .ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier)

let init (connectionString: string) (actorApi: IActor) =
    Log.Information("init query side")

    let ctx = Sql.GetDataContext(connectionString)

    let offsetCount =
        query {
            for o in ctx.Main.Offsets do
                where (o.OffsetName = "Users")
                select o.OffsetCount
                exactlyOne
        }

    let source =
        (readJournal actorApi.System)
            .EventsByTag("default", Offset.Sequence(offsetCount))

    System.Threading.Thread.Sleep(100)

    Log.Information("Journal started")
    let subQueue = Source.queue OverflowStrategy.Fail 256

    let subSink = (Sink.broadcastHub 256)

    let runnableGraph = subQueue |> Source.toMat subSink Keep.both

    let queue, subRunnable = runnableGraph |> Graph.run (actorApi.Materializer)

    source
    |> Source.recover (fun ex ->
        Log.Error(ex, "Error during event reading pipeline")
        None)
    |> Source.runForEach actorApi.Materializer (handleEventWrapper connectionString queue)
    |> Async.StartAsTask
    |> ignore

    System.Threading.Thread.Sleep(1000)
    Log.Information("Projection init finished")
    subRunnable
