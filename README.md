# Katerini

This is an example monorepo that aims to prove that you can build a solution that can scale globally
without an overly complex architecture and without sacrificing developer experience.

## Architecture

To keep in line with the aim of this project, we try to keep the architecture very simple.
It has to be simple enough to keep in your head at all times and to explain to someone within minutes.

Our code consists of a website, a background service and an integration api to abstract / centralize any calls to 3rd party apis.

The aim is to allow both the website and service to scale horizontally and to do that we need to make sure:
 - Communication between them should be via messages. (rabbit mq)
 - Caching has to be via a dedicated caching solution. (redis)
 - Data is saved in a sql server. (mssql)

> **TL;DR**: Applications should scale horizontally, infrastructure should scale vertically (or be IAAS)

## Getting started

Before you can run the application locally, you need to:

1. Build the docker images and run docker-compose to get the infrastructure up and running.
      ```powershell
      dotnet fsi build.fsx run
      ```
2. Create / apply migrations for the database:
      ```powershell
      dotnet fsi build.fsx db
      ```
3. Setup the reverse proxy so you get nice subdomains:
   - Open a notepad as administrator
   - Open file ```%SystemRoot%\System32\drivers\etc\hosts```
   - Add the following text in the end of the file:
     ```text
     # Added by Katerini project
     127.0.0.1 website.katerini.local       # the website running locally
     127.0.0.1 messaging.katerini.local     # a web UI to monitor messages sent via RabbitMQ
     127.0.0.1 logs.katerini.local          # a web UI to monitor logs powered by Seq  
     127.0.0.1 caching.katerini.local       # a web UI to monitor caching powered by Redis
     # 127.0.0.1 mail.katerini.local        # TODO a web UI to monitor the mailserver
     # End of section
     ```

## Monorepo setup

Certain types of files will require different types of approvals. Read the list and plan your PRs accordingly.

| Approval groups      | Comment                                                                                                       |
|----------------------|---------------------------------------------------------------------------------------------------------------|
| Devops               | Platform engineer that understands the infrastructure                                                         |
| Architect / Lead Dev | Expert software developer, good understanding of the whole system, excellent understanding of their subsystem |
| Senior Developer     | Advanced software developer, decent understanding of the whole system, good understanding of their subsystem  |
| Developer            | Software developer, limited understanding of the whole system, decent understanding of their subsystem        |

---

| Approval files / folders | Quality Approval (1st pass) | Domain Approval (2nd pass) | Comments                                                                                                                                           |
|--------------------------|-----------------------------|----------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------|
| ./infrastructure/*       | Devops                      | Architect                  | The recipe for the production architecture. Changes in this folder mean **major** changes in the application.                                      |   
| Dockerfile               | Lead Dev                    | Devops                     | A significant change happened, co-ordinate with your Lead Dev and they will figure out if escalation is needed.                                    | 
| appsettings.*.json       | Lead Dev                    | Architect                  | A significant change happened, possibly need to coordinate changes to docker-compose and /infrastructure to get it working in other environments   |
| complicated subsystem    | Lead Dev                    | Specific Senior Developer  | In the case of a complicated subsystem, identify specific developers with domain knowledge and create an approval group for them                   |
| Program.cs / Program.fs  | Senior Developer            | Lead Dev                   | Contains **_ALL_** dependency injection setup, and in the case of the website **_ALL_** the routes. Changes here are visible to clients.           |
| any other file           | Developer                   | Senior Dev                 | Assuming none of the above files changed then the commit very simple and possibly has no changes a user can perceive                               |

**_Quality Approval_**: "Is this code clear, easy to read, performant and correct? Does it have tests?"
 
**_Domain Approval_**: "Does this code do what we want it to do? Is there a simpler way to do the same thing?"

## Infrastructure

To keep in line with the aim of the project we try and separate the development and infrastructure concerns. 
The idea is to minimize cognitive load for both Software engineers and Platform engineers (devops)

- If you want to run the code locally,    all you need to understand is the docker-compose file and the dockerfiles it uses.
- If you want to deploy it to production, all you need to understand is the docker files and the infrastructure folder.  

There is a bit of overlap in the dockerfiles, i.e everyone needs to be aware of them, but that should be it. 

## Code

These are the main projects:

### Katerini.Website

The website is built using asp.net, giraffe, htmx and bulma. 
The goal is to keep the code F# as much as possible and to avoid complicated javascript/css build systems.

> This is not going to be a SPA (single page application) but instead a multiple page application running on the server. 

- To understand/remember the concept have a look at this free online book: https://hypermedia.systems/book/contents/
- To learn htmx have a look at its documentation: https://htmx.org/docs/

### Katerini.Service

The service is built using standard C# and standard .net 6 coding practices.
It will have multiple background services (workers)

### Katerini.IntegrationApi

This is a standard asp.net api that acts as an intermediary between our applications and 3rd party apis

### Katerini.Database

This is a console application and its only responsibility is managing the database migration.

## CONTENTS

- [Introduction](./README.md)
- [Decisions](./decisions/README.md)
- [Infrastructure](./infrastructure/README.md)
- [Database](./source/Katerini.Database/Scripts/README.md)
- [Messaging](./source/Katerini.Core.Messaging/README.md)

