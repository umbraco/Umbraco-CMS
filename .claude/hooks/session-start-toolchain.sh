#!/bin/bash
# SessionStart hook: checks the toolchain this branch pins (global.json, .nvmrc).
# Cloud sessions install what is missing; local sessions only report it so Claude asks first.
# CI workflows set up their own toolchain and can't ask the user anything.
[ -n "${GITHUB_ACTIONS:-}" ] && exit 0
cd "${CLAUDE_PROJECT_DIR:-.}" || exit 0
remote=false
[ "${CLAUDE_CODE_REMOTE:-}" = true ] && remote=true
missing=()

if [ -f global.json ] && ! dotnet --version >/dev/null 2>&1; then
    sdk=$(sed -n 's/.*"version": *"\([^"]*\)".*/\1/p' global.json | head -1)
    if $remote; then
        if curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --jsonfile global.json --install-dir /usr/share/dotnet >/dev/null 2>&1; then
            [ -n "${CLAUDE_ENV_FILE:-}" ] && echo 'export DOTNET_ROOT=/usr/share/dotnet PATH="/usr/share/dotnet:$PATH"' >> "$CLAUDE_ENV_FILE"
        else
            missing+=(".NET SDK $sdk (global.json): install failed")
        fi
    else
        missing+=(".NET SDK $sdk (global.json)")
    fi
fi

nvmrc=src/Umbraco.Web.UI.Client/.nvmrc
if [ -f "$nvmrc" ]; then
    want=$(tr -d '[:space:]v' < "$nvmrc")
    have=$(node --version 2>/dev/null | tr -d v)
    case "$have" in
        "$want" | "$want".*) ;;
        *)
            if $remote; then
                dir=$(ls -d /opt/node"$want" /opt/node-v"$want" /opt/node-v"$want".* 2>/dev/null | sort -V | tail -1)
                if [ -z "$dir" ]; then
                    ver=$(curl -fsSL https://nodejs.org/dist/index.json | jq -r --arg w "v$want" '[.[] | select(.version == $w or (.version | startswith($w + ".")))][0].version // empty')
                    case "$(uname -m)" in aarch64 | arm64) arch=arm64 ;; *) arch=x64 ;; esac
                    dir=/opt/node-$ver
                    { [ -n "$ver" ] && mkdir -p "$dir" && curl -fsSL "https://nodejs.org/dist/$ver/node-$ver-linux-$arch.tar.xz" | tar -xJ -C "$dir" --strip-components=1; } \
                        || { missing+=("Node $want (.nvmrc): install failed"); dir=; }
                fi
                [ -n "$dir" ] && [ -n "${CLAUDE_ENV_FILE:-}" ] && echo "export PATH=\"$dir/bin:\$PATH\"" >> "$CLAUDE_ENV_FILE"
            else
                missing+=("Node $want (.nvmrc; found ${have:-none})")
            fi
            ;;
    esac
fi

if [ ${#missing[@]} -gt 0 ]; then
    echo "This branch pins tooling that is not available:"
    printf -- '- %s\n' "${missing[@]}"
    $remote || echo "Ask the user before installing or switching versions."
fi
exit 0
