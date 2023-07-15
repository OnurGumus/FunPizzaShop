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
    val instant: instant: Instant -> JsonValue

module DefeaultDecode =
    val instant: Decoder<Instant>

val extraThoth: ExtraCoders
val userMessageEncode: Encoder<Common.Event<User.Event>>
val userMessageDecode: Decoder<Common.Event<User.Event>>
val deliveryMessageEncode: Encoder<Common.Event<Delivery.Event>>
val deliveryMessageDecode: Decoder<Common.Event<Delivery.Event>>
val orderMessageEncode: Encoder<Common.Event<Order.Event>>
val orderMessageDecode: Decoder<Common.Event<Order.Event>>
val orderSagaMessageEncode: Encoder<OrderSaga.Event>
val orderSagaMessageDecode: Decoder<OrderSaga.Event>
val deliverySagaMessageEncode: Encoder<DeliverySaga.Event>
val deliverySagaMessageDecode: Decoder<DeliverySaga.Event>
/// State encoding
val userStateEncode: Encoder<User.State>
val userStateDecode: Decoder<User.State>
val orderStateEncode: Encoder<Order.State>
val orderStateDecode: Decoder<Order.State>
val deliveryStateEncode: Encoder<Delivery.State>
val deliveryStateDecode: Decoder<Delivery.State>
val orderSagaStateEncode: Encoder<OrderSaga.State>
val orderSagaStateDecode: Decoder<OrderSaga.State>
val deliverySagaStateEncode: Encoder<DeliverySaga.State>
val deliverySagaStateDecode: Decoder<DeliverySaga.State>

type ThothSerializer =
    new: system: ExtendedActorSystem -> ThothSerializer
    inherit SerializerWithStringManifest
    override Identifier: int
    override ToBinary: obj: obj -> byte array
    override Manifest: o: obj -> string
    override FromBinary: bytes: byte array * manifest: string -> obj
