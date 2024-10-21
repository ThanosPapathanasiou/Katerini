#!/bin/bash
set -euo pipefail

# Check if an environment was passed as an argument
if [ $# -eq 0 ]; then
    echo "Usage: $0 <version> [command]"
    exit 1
fi

VERSION=$1
SOLUTION="katerini"

# if version is not deployed then exit.
if [ ! -d "/home/$USER/$SOLUTION/$VERSION/" ]; then
    echo "Error: The version $FOLDER_PATH has not been deployed."
    exit 1 
fi

# create network
[ ! "$(docker network ls | grep katerini_network)" ] && docker network create -d bridge katerini_network

# TODO setup database upgrade
# docker load --quiet --input /home/$USER/$SOLUTION/$VERSION/katerini.database.$VERSION.tar.gz
# [ ! "$(docker ps -a | grep katerini.database.$VERSION)" ] && docker run --quiet -d --restart unless-stopped --network katerini_network --env-file /home/$USER/$SOLUTION/$VERSION/environment.env --name katerini.database.$VERSION katerini.database:$VERSION
# docker start katerini.database.$VERSION
# sleep 5

# TODO setup service 
# docker load --quiet --input /home/$USER/$SOLUTION/$VERSION/katerini.service.$VERSION.tar.gz
# [ ! "$(docker ps -a | grep katerini.website.$VERSION)" ] && docker run --quiet -d --restart unless-stopped --network katerini_network --env-file /home/$USER/$SOLUTION/$VERSION/environment.env --name katerini.service.$VERSION katerini.service:$VERSION
# docker start katerini.service.$VERSION
# sleep 5
# make proxy point to the correct version.


# setup website
KATERINI_WEBSITE_PORT=$(shuf -i 10000-20000 -n 1)
docker load --quiet --input /home/$USER/$SOLUTION/$VERSION/katerini.website.$VERSION.tar.gz
[ ! "$(docker ps -a | grep katerini.website.$VERSION)" ] && docker run --quiet -d --restart unless-stopped --network katerini_network --env-file /home/$USER/$SOLUTION/$VERSION/environment.env --name katerini.website.$VERSION -p $KATERINI_WEBSITE_PORT:8080 katerini.website:$VERSION
docker start katerini.website.$VERSION
sleep 5

# setup proxy
export VERSION=$VERSION
EXPORT KATERINI_WEBSITE_PORT=8080 # $KATERINI_WEBSITE_PORT
envsubst '${VERSION} {KATERINI_WEBSITE_PORT}' < template_nginx.conf > nginx.conf 
docker load --quiet --input /home/$USER/$SOLUTION/katerini.proxy.tar.gz
[ ! "$(docker ps -a | grep katerini.proxy)" ] && docker run --quiet -d --restart unless-stopped --network katerini_network -v /home/$USER/$SOLUTION/nginx.conf:/etc/nginx/nginx.conf --name katerini.proxy -p 80:80  katerini.proxy
docker start katerini.proxy
sleep 5

exit 1 # TODO: remove this 

# at this point there should be two versions 

while true
do
  # Attempt to get the status of the server
  response=$(curl --connect-timeout 5 --silent -o /dev/null -w "%{http_code}" "localhost/version" 2>/dev/null || { response=000; })
  if [ "$response" = 000 ]; then
    echo "Proxy is not running, starting it..."
  elif [ "$response" = 502 ]; then
    echo "Website is not running, starting it..."
  elif [ "$response" = 200 ]; then 
    version_running=$(curl --connect-timeout 5 --silent localhost/version 2>/dev/null || echo "ERROR_RETRIEVING_VERSION")
    if [ "$version_running" = "$VERSION" ]; then 
      echo "Current version is running."
      echo "Success"
      break;
    elif [ "$version_running" = "ERROR_RETRIEVING_VERSION" ]; then
      echo "There was an error retrieving the version currently running..."
      exit 1
    else 
      echo "Do the deployment"
    fi
  fi
  sleep 5
  echo "Trying again..." 
done

# stopping other versions

exit 0
