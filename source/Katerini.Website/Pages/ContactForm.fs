module Katerini.Website.Pages.ContactForm

open System
open System.Transactions

open Giraffe
open Giraffe.ViewEngine

open Katerini.Core.Messaging.Messages
open Microsoft.AspNetCore.Http

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

let initialEmail     : Email     = { Value = Initial ; Name = nameof(Email)     ; Label = "Email"      ; Url = "/contact-form/email"     }
let initialFirstName : FirstName = { Value = Initial ; Name = nameof(FirstName) ; Label = "Name" ; Url = "/contact-form/firstname" }
let initialLastName  : LastName  = { Value = Initial ; Name = nameof(LastName)  ; Label = "Surname"  ; Url = "/contact-form/lastname"  }

// ---------------------------------
// Validations
// ---------------------------------

let validateEmail (rawEmail: string) : Email =
    match rawEmail with
    | s when String.IsNullOrWhiteSpace s        -> { initialEmail with Value = Invalid (s, "Field is mandatory.")}
    | s when s.Contains("@") && s.Contains(".") -> { initialEmail with Value = Valid s}
    | s                                         -> { initialEmail with Value = Invalid (s, "Invalid Email Address") } 

let validateFirstName (rawFirstName: string) : FirstName =
    match rawFirstName with
    | s when String.IsNullOrWhiteSpace s -> { initialFirstName with Value = Invalid (s, "Field is mandatory.")}
    | s                                  -> { initialFirstName with Value = Valid s}

let validateLastName (rawLastName: string) : LastName =
    match rawLastName with
    | s when String.IsNullOrWhiteSpace s -> { initialLastName with Value = Invalid (s, "Field is mandatory.")}
    | s                                  -> { initialLastName with Value = Valid s}


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

        textFieldComponent contactInformation.Email
        textFieldComponent contactInformation.FirstName
        textFieldComponent contactInformation.LastName

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
                    contactFormComponent { Email = initialEmail; FirstName = initialFirstName; LastName = initialLastName }
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
            let email     = ctx.Request.Form[initialEmail.Name]     |> string |> validateEmail
            let firstName = ctx.Request.Form[initialFirstName.Name] |> string |> validateFirstName
            let lastName  = ctx.Request.Form[initialLastName.Name]  |> string |> validateLastName

            match email.Value, firstName.Value, lastName.Value with
            | Valid validEmail, Valid validFirstName, Valid validLastName ->
                let message = ContactUsMessage(validFirstName, validLastName, validEmail)

                use scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
                do! outbox.AddToOutboxAsync(message)
                scope.Complete()

                let html = contactFormComponent { Email = initialEmail; FirstName = initialFirstName; LastName = initialLastName }
                let view = htmlView html
                return! view next ctx

            | _ ->

                let html = contactFormComponent { Email = email; FirstName = firstName; LastName = lastName }
                let view = htmlView html
                return! view next ctx
        }

let ``POST /contact-form/email`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let email = ctx.Request.Form[initialEmail.Name] |> string |> validateEmail
            let html  = textFieldComponent email
            let view  = htmlView html

            return! view next ctx
        }

let ``POST /contact-form/firstname`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let firstName  = ctx.Request.Form[initialFirstName.Name] |> string |> validateFirstName
            let html       = textFieldComponent firstName
            let view       = htmlView html

            return! view next ctx
        }

let ``POST /contact-form/lastname`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let lastName   = ctx.Request.Form[initialLastName.Name] |> string |> validateLastName
            let html       = textFieldComponent lastName
            let view       = htmlView html

            return! view next ctx
        }
