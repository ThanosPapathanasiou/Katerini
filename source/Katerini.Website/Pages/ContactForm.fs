module Katerini.Website.Pages.ContactForm

open System
open System.Collections.Generic
open System.Transactions

open Giraffe
open Giraffe.ViewEngine

open Microsoft.AspNetCore.Http

open Katerini.Core.Messaging.Messages
open Katerini.Core.Outbox
open Katerini.Website.Pages.Htmx
open Katerini.Website.Pages.BaseView
open Katerini.Website.Pages.Components.TextField

// ---------------------------------
// Models
// ---------------------------------

type Email     = TextField
type FirstName = TextField
type LastName  = TextField
type ContactInformation = {
    Email     : Email
    FirstName : FirstName
    LastName  : LastName
}

let Email     = nameof(Unchecked.defaultof<ContactInformation>.Email)
let FirstName = nameof(Unchecked.defaultof<ContactInformation>.FirstName)
let LastName  = nameof(Unchecked.defaultof<ContactInformation>.LastName)

let data : IDictionary<string, TextFieldMetadata> = dict [
    (Email,     { Name = Email;     Label = "Email";      Url = "/contact-form/email" })
    (FirstName, { Name = FirstName; Label = "First Name"; Url = "/contact-form/firstname" })
    (LastName,  { Name = LastName;  Label = "Last Name";  Url = "/contact-form/lastname" })
]

// ---------------------------------
// Validations
// ---------------------------------

let validateEmail (rawEmail: string) : Email =
    match rawEmail with
    | s when String.IsNullOrWhiteSpace s -> Invalid (s, "Field is mandatory.")
    | s when s.Contains("@") && s.Contains(".") -> Valid s
    | s  -> Invalid (s, "Invalid Email Address") 

let validateFirstName (rawFirstName: string) : FirstName =
    match rawFirstName with
    | s when String.IsNullOrWhiteSpace s -> Invalid (s, "Field is mandatory.")
    | s -> Valid s

let validateLastName (rawFirstName: string) : LastName =
    match rawFirstName with
    | s when String.IsNullOrWhiteSpace s -> Invalid (s, "Field is mandatory.")
    | s -> Valid s


// ---------------------------------
// Views
// ---------------------------------

let contactFormComponent (contactInformation : ContactInformation) =
    form [
        _name        (nameof ContactInformation)
        _hxPost      "/contact-form"
        _hxSwap      "outerHTML"
        _hyperScript "on submit set me.submitting to true wait for htmx:afterOnLoad from me set me.submitting to false"
    ] [

        textFieldComponent contactInformation.Email     data[Email]
        textFieldComponent contactInformation.FirstName data[FirstName]
        textFieldComponent contactInformation.LastName  data[LastName]

        div [ _classes [ Bulma.field; Bulma.``is-grouped`` ] ] [
            button [
                _classes [ Bulma.button; Bulma.``is-link`` ]
                _type "submit"
                _hyperScript "on click add .is-loading to me"
            ] [ Text "Submit" ]
        ]
    ]

let contactFormView =
    let subtitle      = "A simple Form 'component' example"
    let contents      = [
        section [ _classes [ Bulma.section ] ] [
            div [ _classes [ Bulma.container ] ] [
                div [ _classes [ Bulma.``is-full-desktop`` ] ] [
                    h3 [ _classes [Bulma.subtitle ; Bulma.``is-3``] ] [ Text "Signup Form" ]
                    contactFormComponent { Email = Initial; FirstName = Initial; LastName = Initial }
                ]
            ]
        ]
    ]
    createPage subtitle contents

// ---------------------------------
// "Controllers"
// ---------------------------------
let ``GET  /contact-form`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let view = htmlView contactFormView
            return! view next ctx
        }

let ``POST /contact-form`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {

            // dependency injection
            let outbox         = ctx.GetService<IOutboxService>()

            // validations
            let emailValue     = ctx.Request.Form[Email]     |> string |> validateEmail
            let firstNameValue = ctx.Request.Form[FirstName] |> string |> validateFirstName
            let lastNameValue  = ctx.Request.Form[LastName]  |> string |> validateLastName

            match emailValue, firstNameValue, lastNameValue with
            | Valid validEmail, Valid validFirstName, Valid validLastName ->
                let message = ContactUsMessage(validFirstName, validLastName, validEmail)

                use scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
                do! outbox.AddToOutboxAsync(message)
                scope.Complete()

                let html = contactFormComponent { Email=Initial; FirstName=Initial; LastName=Initial }
                let view = htmlView html
                return! view next ctx

            | _ ->

                let html = contactFormComponent { Email=emailValue; FirstName=firstNameValue; LastName=lastNameValue }
                let view = htmlView html
                return! view next ctx
        }

let ``POST /contact-form/email`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let emailValue = ctx.Request.Form[Email] |> string |> validateEmail
            let html       = textFieldComponent emailValue data[Email]
            let view       = htmlView html

            return! view next ctx
        }

let ``POST /contact-form/firstname`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let firstName  = ctx.Request.Form[FirstName] |> string |> validateFirstName
            let html       = textFieldComponent firstName data[FirstName]
            let view       = htmlView html

            return! view next ctx
        }

let ``POST /contact-form/lastname`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let lastName   = ctx.Request.Form[LastName] |> string |> validateLastName
            let html       = textFieldComponent lastName data[LastName]
            let view       = htmlView html

            return! view next ctx
        }
