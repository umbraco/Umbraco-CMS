#!/bin/bash
set -e

# Fix ownership of bind-mounted directories so the app can write to them
mkdir -p /app/wwwroot/media /app/wwwroot/scripts /app/wwwroot/css /app/Views /app/umbraco
chown -R "$APP_UID:$APP_UID" /app/wwwroot/media /app/wwwroot/scripts /app/wwwroot/css /app/Views /app/umbraco

# Drop privileges and run the application as the app user
exec gosu $APP_UID dotnet UmbracoProject.dll
