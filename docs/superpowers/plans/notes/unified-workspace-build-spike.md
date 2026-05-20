# Spike: unified workspace Vite build

**Date:** 2026-05-19
**Status:** Prototype done. **Finding flips the recommendation.** Original estimate (30–50 % build-time win) was wrong. A real prototype shows the unified build is *4.5× slower* than the current per-workspace setup, while a much simpler change — running the existing per-workspace builds in parallel — wins **−43 %** with zero topology risk.

## TL;DR

Three approaches measured on jov's M-series Mac:

| Approach | Wall-clock | CPU % | Verdict |
|---|---:|---:|---|
| Serial per-workspace (`npm run build:workspaces`, current) | 48.7 s | 172 % | baseline |
| **Parallel per-workspace (`xargs -P 8`)** | **27.8 s** | **441 %** | **−43 %, ship this** |
| Unified single Vite config | 3 m 37 s | 110 % | +345 %, dead end |

The unified Rollup graph collapses to a single-threaded chunking pass; the 58-workspace fan-out parallelises naturally across cores. The win we thought required topology-shifting comes for free from `xargs -P` / a small Node fan-out script.

## Why the unified build is slower

Rollup's chunking algorithm processes the full module graph in a single pass. With 215 entries across 58 workspaces walked from one Rollup instance:

- One Rollup process pinned to ~1.1 cores throughout the build (vs ~1.7 cores avg per per-workspace invocation, multiplied across many invocations).
- Inter-entry dependency analysis grows with entry count.
- Source-map generation across the unified graph is no faster, even with sourcemaps disabled (3 m 37 s → 3 m 38 s — no difference).
- Cross-workspace externalization (`external: [/^@umbraco-cms/]`) keeps the chunk graph identical to today, so the only theoretical win was Vite startup amortization (~12 s saved across 58 invocations). That win is dwarfed by Rollup's serialized chunking cost on the larger graph.

The output topology mostly survives — locale chunks land in `packages/core/`, monaco's `azcli` lands in `external/monaco-editor/`. But a small `dist-cms/shared/` directory appears with cross-workspace deduplicated chunks that didn't exist in the per-workspace layout. Those would need `manualChunks` plumbing to relocate. Not worth fixing given the wall-clock regression.

## What I prototyped

Committed for reproducibility (small, easy to revert):

- `src/Umbraco.Web.UI.Client/devops/build/build-workspaces-parallel.js` — cross-platform Node script that fans out `vite build` across all 58 workspaces using `child_process.spawn`. Concurrency defaults to `min(8, cpu_count)`. Drop-in for `npm run build:workspaces`.

Not committed (proved non-viable, deleted from working tree):

- `vite.unified.config.ts` — root unified Vite config with all 215 entries.
- `devops/build/discover-workspaces.js` — static parser of every per-workspace `vite.config.ts` that fed the unified entry map.

The discovery script worked correctly (58 workspaces, 215 entries, FS-side-effects detected). The blocker is purely the Rollup runtime cost.

## Recommendation

**Ship the parallel runner; abandon the unified config.**

1. Add `build:workspaces:parallel` to `package.json` pointing at the Node script.
2. Replace `npm run build:workspaces` with the parallel runner inside `build:for:cms`.
3. Keep this note as the record of *why* the unified approach was rejected, so a future maintainer doesn't re-attempt it without checking the prior numbers.

## Followups not in scope here

- The 27.8 s number could improve further by caching `tsc --build` between runs and by hoisting the `tsc --project src/tsconfig.build.json` step (which is the single biggest serial cost, ~10 s on its own).
- Cross-workspace dedup (drop externalization → true unified bundle, smaller dist-cms) remains a separate v18 question. The runtime-perf wins from that are independent of this build-time investigation; track via the AB#67983 follow-up and #21152.
