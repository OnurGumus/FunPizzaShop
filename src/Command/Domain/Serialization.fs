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

module DefaultEncode =
    let instant (instant : Instant) =
        Encode.datetime (instant.ToDateTimeUtc() )

module DefeaultDecode =
    let instant : Decoder<Instant> = Decode.datetimeUtc |> Decode.map(Instant.FromDateTimeUtc)

let extraThoth =
    Extra.empty
    |> Extra.withInt64
    |> Extra.withDecimal
    |> Extra.withCustom (DefaultEncode.instant) DefeaultDecode.instant


type ThothSerializer(system: ExtendedActorSystem) =
    inherit SerializerWithStringManifest(system)

    override _.Identifier = 1712

    override _.ToBinary(o) =
        
        match o with
        | e -> 
            Log.Fatal("shouldn't happen {e}", e)
            Environment.FailFast("shouldn't happen")
            failwith "shouldn't happen"
        |> Encode.toString 4 
        |> Encoding.UTF8.GetBytes 

    override _.Manifest(o: obj): string = 
        match o with

        | _ -> o.GetType().FullName

    override _.FromBinary(bytes: byte[], manifest: string): obj =
        let decode decoder = 
            Encoding.UTF8.GetString(bytes)
            |> Decode.fromString decoder
            |> function
            | Ok res -> res
            | Error er -> raise(new SerializationException(er))

       
        match manifest with
        | _ ->  
            Log.Fatal("manifest {manifest} not found", manifest)
            Environment.FailFast("shouldn't happen")
            raise(new SerializationException())

