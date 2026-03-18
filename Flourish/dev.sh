#!/usr/bin/env bash
# dev.sh — start local dev environment (DB via Docker, app via dotnet run)
set -e

# Support Colima on macOS
if [[ -S "$HOME/.colima/default/docker.sock" ]]; then
  export DOCKER_HOST="unix://${HOME}/.colima/default/docker.sock"
fi

echo "▶ Starting PostgreSQL..."
docker compose up -d db

echo "⏳ Waiting for PostgreSQL to be ready..."
until docker compose exec -T db pg_isready -U postgres -q 2>/dev/null; do
  sleep 1
done
echo "✓ PostgreSQL ready"

echo "▶ Starting Flourish..."
dotnet run
