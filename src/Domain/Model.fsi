module FunPizzaShop.Domain.Model

open System
open Fable.Validation
open FsToolkit.ErrorHandling
open Thoth.Json

val extraEncoders: ExtraCoders
val inline forceValidate: e: Result<'a, 'b list> -> 'a

type Predicate =
    | Greater of string * int64
    | GreaterOrEqual of string * int64
    | Smaller of string * int64
    | SmallerOrEqual of string * int64
    | Equal of string * obj
    | NotEqual of string * obj
    | And of Predicate * Predicate
    | Or of Predicate * Predicate
    | Not of Predicate

type Version =
    | Version of int64

    member Value: int64
    member Zero: Version

type ShortStringError =
    | EmptyString
    | TooLongString

type ShortString =
    private
    | ShortString of string

    member Value: string
    static member TryCreate: s: string -> Result<ShortString, ShortStringError list>
    static member Validate: s: ShortString -> ShortString

type LongStringError = EmptyString

type LongString =
    private
    | LongString of string

    member Value: string
    static member TryCreate: s: string -> Result<LongString, LongStringError list>
    static member Validate: s: LongString -> LongString

module Pizza =
    /// <summary>
    /// Represents a pre-configured template for a pizza a user can order
    /// </summary>
    [<CLIMutable>]
    type PizzaSpecial =
        { Id: int64
          Name: string
          BasePrice: decimal
          Description: string
          ImageUrl: string }

        member FormattedBasePrice: string

    [<CLIMutable>]
    type Topping =
        { Id: int64
          Name: string
          Price: decimal }

        member FormattedBasePrice: string

    [<CLIMutable>]
    type Pizza =
        { Id: Guid
          Special: PizzaSpecial
          SpecialId: int64
          Size: int64
          Toppings: Topping list }

        static member DefaultSize: int
        static member MinimumSize: int
        static member MaximumSize: int
        member BasePrice: decimal
        member TotalPrice: decimal
        member FormattedTotalPrice: string
        static member CreatePizzaFromSpecial: special: PizzaSpecial -> Pizza
