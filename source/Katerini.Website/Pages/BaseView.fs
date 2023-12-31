module Katerini.Website.Pages.BaseView

// ---------------------------------
// CSS library
// ---------------------------------
open Zanaptak.TypedCssClasses

[<Literal>]
let bulmaUrl = "https://cdn.jsdelivr.net/npm/bulma@0.9.4/css/bulma.min.css"
type Bulma = CssClasses<bulmaUrl>
let BulmaStyle = Bulma.``is-link``
let BulmaStyleColor = "#3e8ed0" 

// ---------------------------------
// Base view layout
// ---------------------------------
open Giraffe.ViewEngine

let inline _classes attributes =
  attributes |> String.concat " " |> _class

let createPage
    (titleText:string)
    (content: XmlNode list) : XmlNode =

    html [
        _lang "en"
        _classes [ Bulma.``is-fullheight`` ]
    ] [

        head [] [
            title []  [ encodedText titleText ]
            meta [ _charset "utf-8" ]
            meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]
            meta [ _name "theme-color"; _content BulmaStyleColor ] 
            link [ _rel "stylesheet"; _type "text/css"; _href bulmaUrl ]
            script [
                    _src "https://unpkg.com/htmx.org@1.9.0"
                    _integrity "sha384-aOxz9UdWG0yBiyrTwPeMibmaoq07/d3a96GCbb9x60f3mOt5zwkjdbcHFnKH8qls"
                    _crossorigin "anonymous" ] []
            script [
                _src "https://unpkg.com/hyperscript.org@0.9.8"
            ] []
        ]

        body [
            _classes [ Bulma.``is-fullheight`` ]
            _style "min-height: 100vh; display: flex; flex-direction: column;"
        ] ([
            nav [ _classes [ Bulma.navbar; BulmaStyle ] ] [
                div [ _class Bulma.``navbar-start`` ] [
                    a [ _class Bulma.``navbar-item`` ; _href "/" ] [ Text "Katerini" ]
                ]

                div [ _class Bulma.``navbar-end`` ] [
                    a [ _class Bulma.``navbar-item`` ; _href "/" ] [ Text "Login" ]
                ]
            ]
        ]
        @ content @
        [
            footer [
                _classes [ Bulma.footer; ]
                _style "margin-top: auto"
            ] [
                div [ _classes [ Bulma.content; Bulma.``has-text-centered``] ] [
                    p [] [
                        Text "Built using F# Giraffe and HTMX"
                    ]
                ]
            ]
        ])
    ]