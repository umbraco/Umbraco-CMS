#!/usr/bin/env bash
# Two-way copy between this folder's memory/ snapshot and the local Claude Code memory directory
# for the main checkout. Run `./sync-memory.sh pull` at the start of a session on a machine that is
# behind, and `./sync-memory.sh push` before committing at the end of a session. Newer files win.
# The memory directory is derived from the main checkout path the way Claude Code names project
# folders; pass it as the second argument or set CLAUDE_MEMORY_DIR if the guess is wrong.
set -euo pipefail
here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
common="$(git -C "$here" rev-parse --path-format=absolute --git-common-dir)"
repo="$(dirname "$common")"
local_dir="${2:-${CLAUDE_MEMORY_DIR:-$HOME/.claude/projects/$(printf '%s' "$repo" | sed 's|[^A-Za-z0-9]|-|g')/memory}}"
case "${1:-}" in
  pull) mkdir -p "$local_dir"; cp -u "$here"/memory/*.md "$local_dir"/; echo "pulled into $local_dir" ;;
  push) cp -u "$local_dir"/*.md "$here"/memory/; echo "pushed from $local_dir" ;;
  *) echo "usage: $0 pull|push"; exit 1 ;;
esac
