# Docker image name for the application
$ImageName="phila/umbraco"

# Docker container name
$ContainerName="umbraco.web"

# Build the docker image
docker build -t $ImageName .

# Launch the image in a container
docker run -d -P --name $ContainerName  $ImageName

# Find out the IP address you need to connect
docker inspect -f "{{ .NetworkSettings.Networks.nat.IPAddress }}" $ContainerName
