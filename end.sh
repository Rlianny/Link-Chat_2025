#!/bin/bash

# Stop containers
docker stop first-container sec-container third-container fourth-container

# Remove containers
docker rm first-container sec-container third-container fourth-container

# Remove virtual network
docker network rm linkchat.net
