
module FunPizzaShop.Client.LoginStore
open ElmishOrder
open FunPizzaShop.MVU.LoginStore

let rec execute order dispatch =
    match order with
    | Order.NoOrder -> ()
   
let store,dispatcher = Program.mkStoreWithOrderExecute init update dispose execute ()
