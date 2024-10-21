#!/bin/bash

docker run -d --restart unless-stopped \
  --name seq \
  -p 5341:80 \
  -e ACCEPT_EULA=Y \
  datalust/seq:latest

docker run -d --restart unless-stopped \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3.12-management

docker run -d --restart unless-stopped \
  --name redis \
  -p 6379:6379 \
  redis:latest

docker run -d --restart unless-stopped \
  --name redis-webui \
  -p 15674:8081 \
  -e REDIS_HOSTS=local:redis:6379 \
  rediscommander/redis-commander:latest

docker run -d --restart unless-stopped \
  --name mssql \
  -p 1433:1433 \
  -e ACCEPT_EULA=Y \
  -e SA_USERNAME=sa \
  -e SA_PASSWORD=YourStrong!Passw0rd \
  mcr.microsoft.com/mssql/server

