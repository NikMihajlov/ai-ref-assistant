#!/bin/bash
set -e

DOCKER_COMPOSE="/usr/local/Cellar/docker-compose/2.12.2/bin/docker-compose"
export DOTNET_ROOT="/usr/local/opt/dotnet/libexec"
export PATH="$DOTNET_ROOT:$PATH"
export DOCKER_HOST="unix://${HOME}/.colima/default/docker.sock"

echo "▶ Starting PostgreSQL..."
$DOCKER_COMPOSE up -d

echo "⏳ Waiting for PostgreSQL to be ready..."
until docker exec flourish-db pg_isready -U postgres -q 2>/dev/null; do
  sleep 1
done
echo "✓ PostgreSQL ready"

echo "▶ Starting Flourish..."
dotnet run
