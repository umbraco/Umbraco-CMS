---
name: umb-update-openapi
description: Regenerate the Management API OpenApi.json for Umbraco CMS from a running backend, and optionally regenerate the backoffice hey-api client so its generated types match. Proactively offer to run this at the end of any work that adds, removes, or changes Management API controllers, endpoints, routes, or their request/response/view models — OpenApi.json and the generated backend-api client go stale the moment the API surface changes, so suggest syncing rather than waiting to be asked. Also use it whenever the user asks to update/refresh/regenerate OpenApi.json or the openapi document, says the generated backend-api / hey-api client is stale or out of date, or has just added an endpoint and needs the client types — including phrasings like "sync the OpenAPI spec", "update the API client", "the openapi doc is out of date", or "regenerate the server API", even if OpenApi.json isn't named explicitly. Does NOT apply to Delivery API changes or to internal service/repository edits that don't alter the Management API surface.
---

# Update Management API OpenApi.json (Umbraco CMS)

Refresh `src/Umbraco.Cms.Api.Management/OpenApi.json` **byte-for-byte** from a running backend's OpenAPI document, then optionally regenerate the backoffice hey-api client so its generated types match.

This replaces the old manual routine (run the app, open OpenAPI UI, copy JSON, paste, fix formatting). The point of automating it is that a byte-for-byte fetch from the canonical endpoint is deterministic — there's no editor reformatting to strip out, and repeated runs produce identical output.

**Run from the repository root** (the directory containing `umbraco.sln`). Bash's working directory persists between calls, but `cd` can trigger a permission prompt — prefer paths relative to the repo root over `cd`.

## Key facts

- **OpenAPI endpoint:** `/umbraco/openapi/management.json`, served over HTTPS with a dev cert. OpenApi is only mapped when the environment is **not Production** — a backend running in Production returns 404 here even though it's healthy.
- **Port:** default **`44339`** (the `launchSettings.json` https profile). Assume it unless the user says otherwise.
- **Target file:** `src/Umbraco.Cms.Api.Management/OpenApi.json` — git-tracked, so git is the safety net for a bad fetch.
- **The mechanical work is a script, not this document.** `npm run generate:openapi` fetches, validates and writes the spec; `npm run generate:server-api` then regenerates the client **from the committed file**. Prefer these over hand-rolled `curl`.
- **Keep those two commands separate — never chain them into one.** The client generator reads the committed `OpenApi.json` and only that. Pointing it at a live server instead has been tried and removed (b8c2a3366fd): it broke base-url handling, and worse, it produced a client with no matching committed schema. Schema and client belong in the same commit.
- **This skill exists for the parts a script can't do:** knowing _when_ to run it, managing the backend lifecycle, and reading the diff.

## Procedure

### 1. Confirm the target exists

Verify `src/Umbraco.Cms.Api.Management/OpenApi.json` is present. If it isn't, you're likely not at the repo root — stop and say so rather than writing a file in the wrong place.

### 2. Make sure a backend is running on 44339

> **The fetched document reflects the _running_ build, not the working tree.** If Management API code changed since the server started, a still-running instance serves a stale spec — you'll get a byte-for-byte-valid file that's silently missing your latest endpoints. If the user changed the API, make sure the instance was (re)built/restarted after those changes; if in doubt, restart it.
>
> **Not every endpoint appears in the document.** Controllers marked `[ApiExplorerSettings(IgnoreApi = true)]` are excluded by design — notably the back-office _security_ endpoints (`BackOfficeController`: login, token, sign-out, authorize, keep-alive). So the absence of, say, a new `keep-alive` endpoint is expected, not a sign of a stale build. Don't use "is my new endpoint in the doc?" as a freshness check unless you've confirmed that controller is actually exposed to the API explorer.

Probe for a live document (short timeout, accept the dev cert):

```bash
CODE=$(curl -sk --max-time 5 -o /dev/null -w "%{http_code}" "https://localhost:44339/umbraco/openapi/management.json")
```

- **`200`** → a backend is already up. Reuse it; do **not** stop it afterward (the user had it running).
- **`000`** → nothing is listening on 44339. Start one yourself, in the background, on 44339, and remember that _you_ started it so you can stop it in step 5. No `--no-build` here on purpose, so a cold repo still works.
- **anything else** → something is already listening on 44339 but isn't serving the expected OpenAPI document. Don't start another instance on the same port; surface the HTTP status and stop. A `404` from an otherwise healthy Umbraco is the common one — that's a backend running in Production, where OpenApi isn't mapped. Say so, and let the user decide whether to restart it in Development rather than restarting it for them.

For the `000` case, start the backend with:

```bash
dotnet run --project src/Umbraco.Web.UI --no-launch-profile -- \
  --environment Development --urls https://localhost:44339
```

Run that with `run_in_background: true` — that also puts the process under the harness's control so step 5 can stop it cleanly. First-run startup (build + boot) can take a couple of minutes — the fetch in step 3 waits for it.

### 3. Fetch byte-for-byte straight into the file

```bash
npm --prefix src/Umbraco.Web.UI.Client run generate:openapi -- --wait
```

The script writes the raw response body straight into the target — that's the byte-for-byte copy — and only after it parses as JSON, so a failed run leaves `OpenApi.json` untouched rather than restoring it after the fact. `--wait` keeps retrying while nothing is listening yet, so a backend you started in step 2 is picked up the moment it's up (no `sleep` loops); against a server that was already running it returns straight away.

Pass a URL as the first argument if the user is on a non-default port.

A non-zero exit is a failed run — report the script's own message rather than retrying. The usual causes are a backend in Production (OpenApi unmapped), a non-`Run` runtime level mid-install/upgrade, or a different app on the port.

### 4. Show the diff

Surface exactly what changed so the user can sanity-check it:

```bash
git diff --stat -- src/Umbraco.Cms.Api.Management/OpenApi.json
```

- **No diff** → the spec is already up to date; say so. **This does not mean the generated client is in sync** — a previous spec change may never have been followed by a regen, so step 6 still applies.
- **A focused diff** → summarize the changed schemas/paths at a high level.
- **A whole-file reformat** → flag it. Byte-for-byte is intentional, but a run that rewrites the _entire_ file means the server's serialization differs from what was committed. That's the canonical format going forward and is fine — just make sure the user knows it's a formatting shift, not hundreds of real API changes.

### 5. Stop the backend if you started it

Only if **you** started it in step 2 (it wasn't already running) — if the probe returned `200`, skip this step and leave the user's server alone.

The instance you launched in step 2 ran with `run_in_background: true`, so the harness owns it as a tracked background task. Stop _that task_ directly, using the background shell's own stop — not a port scan. This is fully cross-platform (no `lsof`, which isn't installed on Windows) and can only ever stop the process you started, never an unrelated one that happens to be bound to 44339.

### 6. Regenerate the backoffice client to check it's in sync

The generated hey-api client is downstream of `OpenApi.json`, but it can drift **independently** of it: a previous spec change that was never followed by a regen leaves a stale client even when step 4 shows no spec diff. So don't gate this on step 4 — regenerating is the only way to know the committed client actually matches the current spec. The generator is deterministic and git-recoverable, so running it against an already-synced client is a no-op (empty diff).

Get the user's agreement before running it — it rewrites generated sources and can be noisy. **Ask once, not twice.** If you got here from a proactive offer (you suggested the sync yourself at the end of some Management API work), the client regen belongs in that same offer — "shall I sync `OpenApi.json` and the generated client?" — and a yes covers this step. Only ask separately if you're mid-run and it genuinely hasn't come up yet:

> "Want me to regenerate the backoffice hey-api client (`npm run generate:server-api`) to confirm it's in sync with `OpenApi.json`? It's a no-op if it already matches."

If yes:

```bash
npm --prefix src/Umbraco.Web.UI.Client run generate:server-api
git diff --stat -- src/Umbraco.Web.UI.Client/src/packages/core/backend-api/
```

Read the resulting diff honestly:

- **No diff** → the client is in sync; done.
- **A diff even though step 4 showed no spec change** → the client had drifted (a prior spec update wasn't regenerated). Surface it — this is exactly the case worth catching, and the regen you just ran fixes it.
- **A diff matching this run's spec changes** → expected; summarize it.

If the user declines, note the client's sync status is now unverified against `OpenApi.json`.

## Notes

- **Don't reformat the fetched JSON.** The user asked for byte-for-byte; the endpoint is the source of truth. If diffs ever look noisy from formatting alone, that's a decision to make with the user (e.g. a normalization pass) — not something to do silently.
- **JSON-valid does not mean fresh.** A well-formed response from a pre-change build is still adopted (see step 2). When correctness matters more than speed, restart the backend so it reflects current source before fetching.
- This skill talks to a live server and, optionally, mutates generated sources — it is not read-only, but it is safe to re-run and git-recoverable. It's idempotent when the spec hasn't changed.
- If the port isn't reachable and starting the backend fails (build errors, DB not configured), surface the `dotnet run` output rather than retrying blindly — the fix is in the app, not the skill.
