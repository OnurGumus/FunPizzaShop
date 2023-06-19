module FunPizzaShop.Domain.Model

open System
open Fable.Validation
open FsToolkit.ErrorHandling
open Thoth.Json
let extraEncoders = Extra.empty |> Extra.withInt64 |> Extra.withDecimal

let inline forceValidate (e) =
    match e with
    | Ok x -> x
    | Error x ->
        let errors = x |> List.map (fun x -> x.ToString()) |> String.concat ", "
        invalidOp errors

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
    member this.Value: int64 = let (Version lng) = this in lng
    member this.Zero: Version = Version 0L

type ShortStringError =
    | EmptyString
    | TooLongString

type ShortString =
    private
    | ShortString of string

    member this.Value = let (ShortString s) = this in s

    static member TryCreate(s: string) =
        single (fun t ->
            t.TestOne s
            |> t.MinLen 1 EmptyString
            |> t.MaxLen 255 TooLongString
            |> t.Map ShortString
            |> t.End)

    static member Validate(s: ShortString) =
        s.Value |> ShortString.TryCreate |> forceValidate

type LongStringError = EmptyString

type LongString =
    private
    | LongString of string

    member this.Value = let (LongString lng) = this in lng

    static member TryCreate(s: string) =
        single (fun t -> t.TestOne s |> t.MinLen 1 EmptyString |> t.Map LongString |> t.End)

    static member Validate(s: LongString) =
        s.Value |> LongString.TryCreate |> forceValidate



module Pizza =
    /// <summary>
    /// Represents a pre-configured template for a pizza a user can order
    /// </summary>
    [<CLIMutable>]
    type PizzaSpecial =
        {
            Id: int64
            Name: string
            BasePrice: decimal
            Description: string
            ImageUrl: string
        }
        member this.FormattedBasePrice = this.BasePrice.ToString("0.00")

    [<CLIMutable>]
    type Topping =
        {
            Id: int64
            Name: string
            Price: decimal
        }
        member this.FormattedBasePrice = this.Price.ToString("0.00")

    [<CLIMutable>]
    type Pizza =
        {
            Id: int64
            Special: PizzaSpecial
            SpecialId: int64
            Size: int64
            Toppings: Topping list
        }
        static member DefaultSize = 12
        static member MinimumSize = 9
        static member MaximumSize = 17

        member this.BasePrice =
            ((decimal) this.Size / (decimal) Pizza.DefaultSize)
            * this.Special.BasePrice

        member this.TotalPrice =
            this.BasePrice
            + (this.Toppings |> List.sumBy(fun t -> t.Price))

        member this.FormattedTotalPrice = this.TotalPrice.ToString("0.00")