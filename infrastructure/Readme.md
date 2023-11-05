## Infrastructure

> **_Only devops / technical architects should have approval rights for this folder._**

This folder contains the majority of production infrastructure for the application.

The only other code (outside of this folder) that we need to be aware of: 

| Type       | Location                                                          |
|------------|-------------------------------------------------------------------|
| Dockerfile | [Katerini.Website](../source/Katerini.Website/Dockerfile)         |
| Dockerfile | [Katerini.Service](../source/Katerini.Service/Dockerfile)         |
| Folder     | [database scripts](../source/Katerini.Database/Scripts/README.md) | 

Any other code that looks like "devops" code and is outside of this folder is only for local development.

