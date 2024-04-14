module Katerini.Website.Pages.Components.TextField

open Giraffe.ViewEngine
open Katerini.Website.Pages.Htmx
open Katerini.Website.Pages.BaseView

type Value          = string
type ErrorMessage   = string
type TextFieldValue = Initial | Invalid of (Value * ErrorMessage) | Valid of Value

type TextField = {
    Name  : string
    Label : string
    Url   : string
    Value : TextFieldValue
}

let textFieldComponent (textField:TextField) =
    let emptySpaceIcon = "&#160;"
    let successIcon    = "✔" 
    let warningIcon    = "⚠"

    let cssClass, value, message, icon =
        match textField.Value with
        | Initial                -> [""]                  , ""   , emptySpaceIcon, emptySpaceIcon
        | Invalid (value, error) -> [Bulma.``is-danger`` ], value, error         , warningIcon
        | Valid    value         -> [Bulma.``is-success``], value, emptySpaceIcon, successIcon

    div [ _classes [ Bulma.field ] ] [
        label [ _class Bulma.label; _for textField.Name ] [ Text textField.Label ]
        div   [ _classes [ Bulma.control; Bulma.``has-icons-right`` ] ] [
            input [
                _type             "text"
                _classes          ([ Bulma.input ] @ cssClass)
                _name             textField.Name
                _title            textField.Name
                _value            value

                _hxPost           textField.Url
                _hxTrigger        "blur delay:200ms"
                _hyperScripts     ["on htmx:beforeRequest if (closest <form/>).submitting then halt end"
                                   "then on htmx:beforeRequest add .is-loading to (closest <div/>)"
                                   "then on htmx:beforeRequest add @disabled to me"
                                   "then on htmx:beforeRequest remove (next <span/>) end"]
                _hxTarget          ("closest ." + Bulma.field)
            ]
            span [ _classes [ Bulma.icon; Bulma.``is-right``; Bulma.``is-small`` ] ] [
                Text icon
            ]
            p [ _classes ([Bulma.help] @ cssClass) ] [ Text message ]
        ]
    ]