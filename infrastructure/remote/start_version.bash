#!/bin/bash
set -euo pipefail

# Check if an environment was passed as an argument
if [ $# -eq 0 ]; then
    echo "Usage: $0 <version>"
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

# setup database upgrade
if ! docker ps -q -f name=katerini.database.$VERSION | grep -q .; then
    echo "katerini.database.$VERSION container is not running... Setting it up."
    docker load --quiet --input /home/$USER/$SOLUTION/$VERSION/katerini.database.$VERSION.tar.gz
    [ ! "$(docker ps -a | grep katerini.database.$VERSION)" ] && docker run --quiet -d --network katerini_network --env-file /home/$USER/$SOLUTION/$VERSION/environment.env --name katerini.database.$VERSION katerini.database:$VERSION
    docker start katerini.database.$VERSION
    sleep 5
    
    echo "katerini.database.$VERSION container started... Database schema is changing..."
fi

# setup service 
if ! docker ps -q -f name=katerini.service.$VERSION | grep -q .; then
    echo "katerini.service.$VERSION container is not running... Setting it up."
    docker load --quiet --input /home/$USER/$SOLUTION/$VERSION/katerini.service.$VERSION.tar.gz
    [ ! "$(docker ps -a | grep katerini.service.$VERSION)" ] && docker run --quiet -d --restart unless-stopped --network katerini_network --env-file /home/$USER/$SOLUTION/$VERSION/environment.env --name katerini.service.$VERSION katerini.service:$VERSION
    docker start katerini.service.$VERSION
    sleep 5
fi

# setup website
if ! docker ps -q -f name=katerini.website.$VERSION | grep -q .; then
    KATERINI_WEBSITE_PORT=$(shuf -i 10000-20000 -n 1)
    echo "katerini.website.$VERSION container is not running... Setting it up."
    docker load --quiet --input /home/$USER/$SOLUTION/$VERSION/katerini.website.$VERSION.tar.gz
    [ ! "$(docker ps -a | grep katerini.website.$VERSION)" ] && docker run --quiet -d --restart unless-stopped --network katerini_network --env-file /home/$USER/$SOLUTION/$VERSION/environment.env --name katerini.website.$VERSION -p $KATERINI_WEBSITE_PORT:8080 katerini.website:$VERSION
    docker start katerini.website.$VERSION
    sleep 5
fi

# setup proxy
if ! docker ps -q -f name=katerini.proxy | grep -q .; then
    echo "katerini.proxy container is not running... Setting it up."
    export VERSION=$VERSION
    envsubst '${VERSION}' < /home/$USER/$SOLUTION/template_nginx.conf > /home/$USER/$SOLUTION/nginx.conf 
    docker load --quiet --input /home/$USER/$SOLUTION/katerini.proxy.tar.gz
    [ ! "$(docker ps -a | grep katerini.proxy)" ] && docker run --quiet -d --restart unless-stopped --network katerini_network -v /home/$USER/$SOLUTION/nginx.conf:/etc/nginx/nginx.conf --name katerini.proxy -p 80:80  katerini.proxy
    docker start katerini.proxy
    sleep 5
fi

# at this point there should be two versions 

while true
do
  # Attempt to get the status of the server
  response=$(curl --connect-timeout 5 --silent -o /dev/null -w "%{http_code}" "localhost/version" 2>/dev/null || { response=000; })
  if [ "$response" = 000 ]; then
    echo "Proxy is not running..."
  elif [ "$response" = 502 ]; then
    echo "Website is not running..."
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
      echo "Doing the deployment."
      echo "Version running is        : $version_running"
      echo "Version to be deployed is : $VERSION"
      # we are using ed because we need to replace the text of the file while keeping the same inode (i.e. actual, in place, file editing.)
      ed -s "/home/$USER/$SOLUTION/nginx.conf" <<EOF
1,\$s/$version_running/$VERSION/g
w
q
EOF

      if ! docker exec katerini.proxy nginx -s reload; then
          echo "Failed to reload Nginx configuration"
          exit 1
      fi

      old_version=$version_running
    fi
  fi
  echo "Trying again..." 
  sleep 5
done

# stopping other versions
if [ -n "${old_version+set}" ]; then
  echo "Stopping old version: '$old_version'"
  echo "Stopped containers with ids:"
  docker stop $(docker ps -q -f name=katerini.*.$old_version)
fi

echo "Version '$VERSION' deployed successfully!" 

exit 0