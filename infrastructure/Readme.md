## Infrastructure

> **_Only devops / technical architects should have approval rights for this folder._**
> 
> **_Any change in the localhost folder should have corresponding changes in the remote folder._**

This folder contains the tools needed to release the solution to production as well as have it run on your local machine.

The only other code (outside of this folder) that we need to be aware of is:
- the dockerfiles of each project
- the build.fsx file 

**There should be no other code that looks like 'Devops' outside of this folder.**

## How to deploy to an environment

The scripts are written in such a way as to deploy the website and service to a single server at a time with zero downtime. 
It assumes that your database, logging, messaging, etc. are hosted somewhere else, probably in a SaaS way. 

This allows you the freedom to scale both vertically and horizontally by just adding a load balancer in front of your website.

After that is done: 
- You can scale your own code vertically by getting a bigger server, deploying there and then retiring the old one.
- You can scale horizontally by just adding a new server as is needed. 

For example, you can deploy to the test-remote environment by running the following:
```bash
~/projects/katerini/infrastructure/remote/deploy.bash test-remote 
```

## How to create a new environment ( add a new server )

Once you've provisioned a server you need to do two things:
1. You will need to make it so that the machine that does the deployment can ssh into the server. 
2. You will need to run the initialization script for that server (it installs docker, creates a user for deployment, etc) 

Once that's done, you will need to create a `environment-name.env` file in the remote folder.
Have a look at the existing ones and follow their example.  

