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

# Drop privileges and run the application as the app user
exec gosu "$APP_UID" dotnet UmbracoProject.dll
