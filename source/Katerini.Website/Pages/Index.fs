module Katerini.Website.Pages.Index

open Giraffe
open Giraffe.ViewEngine

open Microsoft.AspNetCore.Http

open Katerini.Website.Pages.BaseView

// ---------------------------------
// Views
// ---------------------------------

let indexView =
    let title = "Katerini Project"
    let contents      = [
        section [ _classes [ Bulma.section ] ] [
            div [ _classes [ Bulma.container ] ] [
                div [ _classes [ Bulma.box ] ] [
                    div [ _classes [ Bulma.content ] ] [
                        a [ _href "/contact-form" ] [
                            Text "Contact Us!"
                        ]
                        p [] [
                            Text "A simple contact form that uses htmx to validate the input server side!"
                        ]
                    ]
                ]
            ]
        ]
    ]
    createPage title contents

// ---------------------------------
// "Controllers"
// ---------------------------------
let ``GET /`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        htmlView indexView next ctx