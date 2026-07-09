#!/bin/bash
set -e

# Fix ownership of bind-mounted directories so the app can write to them.
# Only run chown -R if the directory is not already owned by APP_UID to avoid
# slow recursive operations on large directories (e.g., media with many files).
fix_ownership() {
    local dir="$1"
    mkdir -p "$dir"
    if [ "$(stat -c '%u' "$dir")" != "$APP_UID" ]; then
        chown -R "$APP_UID:$APP_UID" "$dir"
    fi
}

fix_ownership /app/wwwroot/media
fix_ownership /app/wwwroot/scripts
fix_ownership /app/wwwroot/css
fix_ownership /app/Views
fix_ownership /app/umbraco

# Generate self-signed certificate on first run.
# /https is bind-mounted to ./certs on the host, so aspnetcore.crt is accessible
# for trusting via the trust-cert scripts provided alongside docker-compose.yml.
if [ ! -f /https/aspnetcore.pfx ]; then
    mkdir -p /https
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
      -keyout /https/aspnetcore.key \
      -out /https/aspnetcore.crt \
      -subj "/CN=localhost" \
      -addext "subjectAltName=DNS:localhost"
    openssl pkcs12 -export -out /https/aspnetcore.pfx \
      -inkey /https/aspnetcore.key \
      -in /https/aspnetcore.crt \
      -password "pass:${ASPNETCORE_Kestrel__Certificates__Default__Password}"
    chmod 644 /https/aspnetcore.pfx /https/aspnetcore.crt
fi

# Drop privileges and run the application as the app user
exec gosu "$APP_UID" dotnet UmbracoProject.dll
