#!/bin/bash
set -euo pipefail

export SOLUTION="katerini"
export APPS="katerini.website,katerini.service"
export ENV_VARIABLE_TRIM_START="KATERINI_"
export TAG=$(git rev-parse --short HEAD)

#if [ -n "$(git status --porcelain)" ]; then
#  echo "Repository has uncommitted changes."
#  echo "Please commit before trying to deploy to any remote servers."
#  exit 1
#fi

# Check if an environment was passed as an argument
if [ $# -eq 0 ]; then
    echo "Usage: $0 <environment> [command]"
    exit 1
fi

ENV=$1
ENV_FILE="${ENV}.env"
if [ ! -f "$ENV_FILE" ]; then
    echo "Environment file for ${ENV} not found at ${ENV_FILE}"
    exit 1
fi
source "$ENV_FILE"

cd ../.. # go to root repo level

echo "Preparing deployment folder..."
rm -r -f deployment

mkdir -p deployment/$TAG
# prepare environment file
{ 
  while IFS= read -r line; do
    if [[ $line =~ ^export\ (${ENV_VARIABLE_TRIM_START}) ]]; then
      transformed_line=$(echo "$line" | sed "s/export ${ENV_VARIABLE_TRIM_START}//")
      echo "$transformed_line"
    fi
  done < ./infrastructure/remote/${ENV_FILE}
 echo "VERSION=$(git rev-parse --short HEAD)"
} > ./deployment/$TAG/environment.env

# prepare docker files
IFS=',' read -r -a apps <<< "$APPS"
for app in "${apps[@]}"; do
  tagged_image="${app}:${TAG}"
  echo "Creating package in target directory for $tagged_image image..."
  docker build --quiet -f "./source/$app/Dockerfile" . -t ${tagged_image}
  gzipped_image_path="./deployment/$TAG/$app.$TAG.tar.gz"
  docker save ${tagged_image} | gzip > ${gzipped_image_path}
done

# prepare proxy
echo "Creating package in target directory for katerini.proxy image..."
docker build --quiet -f "./infrastructure/remote/Katerini.Proxy/Dockerfile" . -t katerini.proxy
gzipped_image_path="./deployment/katerini.proxy.tar.gz"
docker save katerini.proxy | gzip > ${gzipped_image_path}
cp ./infrastructure/remote/katerini.proxy/template_nginx.conf ./deployment/template_nginx.conf

# prepare deployment script
cp ./infrastructure/remote/start_version.bash ./deployment/start_version.bash

remote_host="$DEPLOY_USER@$DEPLOY_HOST"
echo "Copying deployment folder files to $remote_host..."

# if apps with current version do not exist on the remote host then copy them over
deploy_dir="/home/$DEPLOY_USER/$SOLUTION/$TAG" 
if ! ssh ${remote_host} "[ -d $deploy_dir ]"; then
  echo "Deploying '$APPS' to '$remote_host' remote host..." 
  ssh ${remote_host} "mkdir -p $deploy_dir;"
  scp -r ./deployment/$TAG/* ${remote_host}:${deploy_dir}    
fi

# if proxy does not exist on the remote host then copy it over
deploy_dir="/home/$DEPLOY_USER/$SOLUTION/" 

if ! ssh ${remote_host} "[ -f $deploy_dir/katerini.proxy.tar.gz ]"; then
  echo "Deploying proxy to '$remote_host' remote host..." 
  scp ./deployment/katerini.proxy.tar.gz ${remote_host}:${deploy_dir}    
fi

scp ./deployment/template_nginx.conf ${remote_host}:${deploy_dir}    
scp ./deployment/start_version.bash ${remote_host}:${deploy_dir}    

exit 0 # TODO: remove this  

# execute the script that will start the version on the server.
ssh ${remote_host} "$deploy_dir/start_version.bash $TAG;"

exit 0