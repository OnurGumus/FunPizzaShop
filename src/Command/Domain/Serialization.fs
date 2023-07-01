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

//Event encoding
let userMessageEncode =
    Encode.Auto.generateEncoder<Common.Event<User.Event>> (extra = extraThoth)
let userMessageDecode =
    Decode.Auto.generateDecoder<Common.Event<User.Event>> (extra = extraThoth)

let deliveryMessageEncode =
    Encode.Auto.generateEncoder<Common.Event<Delivery.Event>> (extra = extraThoth)
let deliveryMessageDecode =
    Decode.Auto.generateDecoder<Common.Event<Delivery.Event>> (extra = extraThoth)

let orderMessageEncode =
    Encode.Auto.generateEncoder<Common.Event<Order.Event>> (extra = extraThoth)
let orderMessageDecode =
    Decode.Auto.generateDecoder<Common.Event<Order.Event>> (extra = extraThoth)

let orderSagaMessageEncode =
    Encode.Auto.generateEncoder<OrderSaga.Event> (extra = extraThoth)
let orderSagaMessageDecode =
    Decode.Auto.generateDecoder<OrderSaga.Event> (extra = extraThoth)

/// State encoding
let userStateEncode = Encode.Auto.generateEncoder<User.State> (extra = extraThoth)
let userStateDecode = Decode.Auto.generateDecoder<User.State> (extra = extraThoth)

let orderStateEncode = Encode.Auto.generateEncoder<Order.State> (extra = extraThoth)
let orderStateDecode = Decode.Auto.generateDecoder<Order.State> (extra = extraThoth)

let deliveryStateEncode = Encode.Auto.generateEncoder<Delivery.State> (extra = extraThoth)
let deliveryStateDecode = Decode.Auto.generateDecoder<Delivery.State> (extra = extraThoth)

let orderSagaStateEncode = Encode.Auto.generateEncoder<OrderSaga.State> (extra = extraThoth)
let orderSagaStateDecode = Decode.Auto.generateDecoder<OrderSaga.State> (extra = extraThoth)



type ThothSerializer(system: ExtendedActorSystem) =
    inherit SerializerWithStringManifest(system)

    override _.Identifier = 1712

    override _.ToBinary(o) =

        match o with
        | :? Common.Event<User.Event> as mesg -> mesg |> userMessageEncode
        | :? Common.Event<Order.Event> as mesg -> mesg |> orderMessageEncode
        | :? Common.Event<Delivery.Event> as mesg -> mesg |> deliveryMessageEncode
        | :? OrderSaga.Event as mesg -> mesg |> orderSagaMessageEncode

        | :? Order.State as mesg -> mesg |> orderStateEncode
        | :? User.State as mesg -> mesg |> userStateEncode
        | :? Delivery.State as mesg -> mesg |> deliveryStateEncode
        | :? OrderSaga.State as mesg -> mesg |> orderSagaStateEncode
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
        | :? Common.Event<Order.Event> -> "OrderMessage"
        | :? Order.State -> "OrderState"
        | :? Common.Event<Delivery.Event> -> "DeliveryMessage"
        | :? Delivery.State -> "DeliveryState"
        | :? OrderSaga.Event -> "OrderSagaMessage"
        | :? OrderSaga.State -> "OrderSagaState"
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
        | "OrderState" -> upcast decode orderMessageDecode

        | "UserMessage" -> upcast decode userMessageDecode
        | "OrderMessage" -> upcast decode orderMessageDecode

        | "DeliveryMessage" -> upcast decode deliveryMessageDecode
        | "DeliveryState" -> upcast decode deliveryStateDecode

        | "OrderSagaMessage" -> upcast decode orderSagaMessageDecode
        | "OrderSagaState" -> upcast decode orderSagaStateDecode

        | _ ->
            Log.Fatal("manifest {manifest} not found", manifest)
            Environment.FailFast("shouldn't happen")
            raise (new SerializationException())
