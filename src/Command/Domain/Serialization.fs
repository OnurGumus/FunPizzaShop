module Command.Serialization

open Command
open Akkling
open Akka.Actor
open Akka.Serialization
open System.Text
open NodaTime
open Thoth.Json.Net
open System.Runtime.Serialization
open Serilog
open System
open FunPizzaShop.Command.Domain

module DefaultEncode =
    let instant (instant: Instant) =
        Encode.datetime (instant.ToDateTimeUtc())

module DefeaultDecode =
    let instant: Decoder<Instant> =
        Decode.datetimeUtc |> Decode.map (Instant.FromDateTimeUtc)

let extraThoth =
    Extra.empty
    |> Extra.withInt64
    |> Extra.withDecimal
    |> Extra.withCustom (DefaultEncode.instant) DefeaultDecode.instant

let userMessageEncode =
    Encode.Auto.generateEncoder<Common.Event<User.Event>> (extra = extraThoth)

let userMessageDecode =
    Decode.Auto.generateDecoder<Common.Event<User.Event>> (extra = extraThoth)

let userStateEncode = Encode.Auto.generateEncoder<User.State> (extra = extraThoth)
let userStateDecode = Decode.Auto.generateDecoder<User.State> (extra = extraThoth)

type ThothSerializer(system: ExtendedActorSystem) =
    inherit SerializerWithStringManifest(system)

    override _.Identifier = 1712

    override _.ToBinary(o) =

        match o with
        | :? Common.Event<User.Event> as mesg -> mesg |> userMessageEncode

        | :? User.State as mesg -> mesg |> userStateEncode
        | e ->
            Log.Fatal("shouldn't happen {e}", e)
            Environment.FailFast("shouldn't happen")
            failwith "shouldn't happen"
        |> Encode.toString 4
        |> Encoding.UTF8.GetBytes

    override _.Manifest(o: obj) : string =
        match o with
        | :? Common.Event<User.Event> -> "UserMessage"
        | :? User.State -> "UserState"
        | _ -> o.GetType().FullName

    override _.FromBinary(bytes: byte[], manifest: string) : obj =
        let decode decoder =
            Encoding.UTF8.GetString(bytes)
            |> Decode.fromString decoder
            |> function
                | Ok res -> res
                | Error er -> raise (new SerializationException(er))

        match manifest with
        | "UserState" -> upcast decode userStateDecode

        | "UserMessage" -> upcast decode userMessageDecode
        | _ ->
            Log.Fatal("manifest {manifest} not found", manifest)
            Environment.FailFast("shouldn't happen")
            raise (new SerializationException())
