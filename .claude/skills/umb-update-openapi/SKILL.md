---
name: umb-update-openapi
description: Regenerate the Management API OpenApi.json for Umbraco CMS from a running backend, and optionally regenerate the backoffice hey-api client so its generated types match. Proactively offer to run this at the end of any work that adds, removes, or changes Management API controllers, endpoints, routes, or their request/response/view models — OpenApi.json and the generated backend-api client go stale the moment the API surface changes, so suggest syncing rather than waiting to be asked. Also use it whenever the user asks to update/refresh/regenerate OpenApi.json or the swagger/openapi document, says the generated backend-api / hey-api client is stale or out of date, or has just added an endpoint and needs the client types — including phrasings like "sync the OpenAPI spec", "update the API client", "the swagger doc is out of date", or "regenerate the server API", even if OpenApi.json isn't named explicitly. Does NOT apply to Delivery API changes or to internal service/repository edits that don't alter the Management API surface.
---

# Update Management API OpenApi.json (Umbraco CMS)

Refresh `src/Umbraco.Cms.Api.Management/OpenApi.json` **byte-for-byte** from a running backend's OpenAPI document, then optionally regenerate the backoffice hey-api client so its generated types match.

This replaces the old manual routine (run the app, open Swagger UI, copy JSON, paste, fix formatting). The point of automating it is that a byte-for-byte fetch from the canonical endpoint is deterministic — there's no editor reformatting to strip out, and repeated runs produce identical output.

**Run from the repository root** (the directory containing `umbraco.sln`). Bash's working directory persists between calls, but `cd` can trigger a permission prompt — prefer paths relative to the repo root over `cd`.

## Key facts

- **OpenAPI endpoint:** `/umbraco/swagger/management/swagger.json`, served over HTTPS with a dev cert (so fetches need `curl -k`).
- **Port:** default **`44339`** (the `launchSettings.json` https profile). Assume it unless the user says otherwise.
- **Target file:** `src/Umbraco.Cms.Api.Management/OpenApi.json` — git-tracked, so git is the safety net for a bad fetch.
- **Client regeneration:** `npm --prefix src/Umbraco.Web.UI.Client run generate:server-api` (delegates to the `@umbraco-backoffice/core` workspace). Reads the freshly-updated `OpenApi.json` and rewrites the generated client under `src/Umbraco.Web.UI.Client/src/packages/core/backend-api/`.

## Procedure

### 1. Confirm the target exists

Verify `src/Umbraco.Cms.Api.Management/OpenApi.json` is present. If it isn't, you're likely not at the repo root — stop and say so rather than writing a file in the wrong place.

### 2. Make sure a backend is running on 44339

> **The fetched document reflects the *running* build, not the working tree.** If Management API code changed since the server started, a still-running instance serves a stale spec — you'll get a byte-for-byte-valid file that's silently missing your latest endpoints. If the user changed the API, make sure the instance was (re)built/restarted after those changes; if in doubt, restart it.
>
> **Not every endpoint appears in the document.** Controllers marked `[ApiExplorerSettings(IgnoreApi = true)]` are excluded by design — notably the back-office *security* endpoints (`BackOfficeController`: login, token, sign-out, authorize, keep-alive). So the absence of, say, a new `keep-alive` endpoint is expected, not a sign of a stale build. Don't use "is my new endpoint in the doc?" as a freshness check unless you've confirmed that controller is actually exposed to the API explorer.

Probe for a live document (short timeout, accept the dev cert):

```bash
CODE=$(curl -sk --max-time 5 -o /dev/null -w "%{http_code}" "https://localhost:44339/umbraco/swagger/management/swagger.json")
```

- **`200`** → a backend is already up. Reuse it; do **not** stop it afterward (the user had it running).
- **`000`** → nothing is listening on 44339. Start one yourself, in the background, on 44339, and remember that *you* started it so you can stop it in step 5. No `--no-build` here on purpose, so a cold repo still works. If `CODE` is neither `200` nor `000`, something is already listening on 44339 but isn't serving the expected OpenAPI document. Don’t try to start another instance on the same port; surface the HTTP status and stop:

```bash
dotnet run --project src/Umbraco.Web.UI --no-launch-profile -- \
  --environment Development --urls https://localhost:44339
```

Run that with `run_in_background: true`. First-run startup (build + boot) can take a couple of minutes — the fetch in step 3 waits for it.

### 3. Fetch byte-for-byte straight into the file, then validate

Curl writes the raw response directly into the target — that's the byte-for-byte copy. `--fail` means an HTTP error (4xx/5xx) won't land an error page in the file, and `--retry-connrefused` lets a just-started server be picked up the moment it's listening (no `sleep` loops). Then confirm it parses as JSON; if it doesn't, restore the committed file with git rather than leaving something broken behind:

```bash
TARGET=src/Umbraco.Cms.Api.Management/OpenApi.json
if curl -sk --fail --retry-connrefused --retry 60 --retry-delay 3 --retry-max-time 300 \
     "https://localhost:44339/umbraco/swagger/management/swagger.json" -o "$TARGET" \
   && node -e "JSON.parse(require('fs').readFileSync('$TARGET','utf8'))"; then
  echo "OK"
else
  echo "FETCH/VALIDATE FAILED — restoring"
  git checkout -- "$TARGET"
fi
```

If it failed: report what happened (a non-`Run` runtime level mid-install/upgrade, or the wrong app on the port are the usual causes) and treat the run as failed — the file is back to its committed state.

### 4. Show the diff

Surface exactly what changed so the user can sanity-check it:

```bash
git diff --stat -- src/Umbraco.Cms.Api.Management/OpenApi.json
```

- **No diff** → already up to date; say so.
- **A focused diff** → summarize the changed schemas/paths at a high level.
- **A whole-file reformat** → flag it. Byte-for-byte is intentional, but a run that rewrites the *entire* file means the server's serialization differs from what was committed. That's the canonical format going forward and is fine — just make sure the user knows it's a formatting shift, not hundreds of real API changes.

### 5. Stop the backend if you started it

Only if **you** started it in step 2 (it wasn't already running), stop it — don't leave a stray process, and never kill a server the user already had up:

```bash
PIDS=$(lsof -ti tcp:44339)
for PID in $PIDS; do
  ps -p "$PID" -o command= | grep -q "Umbraco.Web.UI" && kill "$PID"
done
```

### 6. Offer to regenerate the backoffice client

The generated hey-api client is downstream of `OpenApi.json`, so it's worth regenerating when step 4 produced real changes. **Ask the user** before running it — it rewrites generated sources and can be noisy:

> "OpenApi.json updated. Want me to regenerate the backoffice hey-api client (`npm run generate:server-api`) so the generated types match?"

If yes:

```bash
npm --prefix src/Umbraco.Web.UI.Client run generate:server-api
git diff --stat -- src/Umbraco.Web.UI.Client/src/packages/core/backend-api/
```

Report the generated diff the same way. If the user declines, note the client is now out of sync with `OpenApi.json` until it's regenerated.

## Notes

- **Don't reformat the fetched JSON.** The user asked for byte-for-byte; the endpoint is the source of truth. If diffs ever look noisy from formatting alone, that's a decision to make with the user (e.g. a normalization pass) — not something to do silently.
- **JSON-valid does not mean fresh.** A well-formed response from a pre-change build is still adopted (see step 2). When correctness matters more than speed, restart the backend so it reflects current source before fetching.
- This skill talks to a live server and, optionally, mutates generated sources — it is not read-only, but it is safe to re-run and git-recoverable. It's idempotent when the spec hasn't changed.
- If the port isn't reachable and starting the backend fails (build errors, DB not configured), surface the `dotnet run` output rather than retrying blindly — the fix is in the app, not the skill.
