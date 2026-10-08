#!/usr/bin/env bash
# Two-way copy between this folder's memory/ snapshot and the local Claude Code memory directory
# for the main checkout. Run `./sync-memory.sh pull` at the start of a session on a machine that is
# behind, and `./sync-memory.sh push` before committing at the end of a session. Newer files win.
set -euo pipefail
here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
common="$(git -C "$here" rev-parse --path-format=absolute --git-common-dir)"
repo="$(dirname "$common")"
local_dir="$HOME/.claude/projects/$(printf '%s' "$repo" | sed 's|/|-|g')/memory"
case "${1:-}" in
  pull) mkdir -p "$local_dir"; cp -u "$here"/memory/*.md "$local_dir"/; echo "pulled into $local_dir" ;;
  push) cp -u "$local_dir"/*.md "$here"/memory/; echo "pushed from $local_dir" ;;
  *) echo "usage: $0 pull|push"; exit 1 ;;
esac
