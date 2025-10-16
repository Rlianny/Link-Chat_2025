#!/bin/bash

# Create virtual network
docker network create linkchat.net

# Create and eject containers
gnome-terminal -- bash -c "docker run -it --name first-container --network linkchat.net link.chat"
gnome-terminal -- bash -c "docker run -it --name sec-container --network linkchat.net link.chat"
gnome-terminal -- bash -c "docker run -it --name third-container --network linkchat.net link.chat"
gnome-terminal -- bash -c "docker run -it --name fourth-container --network linkchat.net link.chat"

