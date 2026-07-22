# Cookie-based authentication — Frontend POC (design)

- **Work item:** AB#70018 "Frontend POC" (parent AB#61650 "Cookie-based authentication", epic AB#48415 "CMS Next Major (Breaking changes)")
- **Child tasks:** AB#70019 "Finish Kenn's work on the FE", AB#70020 "Placeholder for /session check", AB#70073 "Test external login providers"
- **Base branch:** `v19/feature/management-api-cookie-auth` (Kenn's backend POC — continuing on it directly)
- **Related feature:** AB#61845 "Cookie-based authentication (phase 2)" — the end goal this POC works toward

## Background

Kenn's POC branch (`v19/feature/management-api-cookie-auth`, 3 commits, net −1,320 lines) established the backend half of pure cookie auth:

1. **Dual-scheme authorization** — every Management API authorization policy now accepts *either* the OpenIddict validation scheme *or* the classic backoffice auth cookie (`Constants.Security.BackOfficeAuthenticationType`), via `BackOfficeAuthPolicyBuilderExtensions.AddAuthenticationSchemes`.
2. **Removed `HideBackOfficeTokensHandler`** (+ its 995-line test) — the v17 `[redacted]`-token / `__Host-` cookie-wrapping interceptor is gone. The login cookie *is* the credential.
3. **Frontend stub** — a single `COOKIE_AUTH_BYPASS = true` flag in `auth.context.ts` that fakes a 1-year in-memory session, always reports authorized, and drops the bearer `auth` callback. Explicitly marked "NOT FOR PRODUCTION".

The backoffice login cookie is already issued today: `POST /umbraco/management/api/v1/security/back-office/login` calls `PasswordSignInAsync`, which sets the ASP.NET Core Identity cookie as the first step of the existing OIDC flow. Kenn's dual-scheme change is what makes the API trust that cookie without a bearer token.

**This spec covers the frontend half:** replacing the bypass stub with a real, purely cookie-based auth client.

## Goal

The backoffice client treats the httpOnly login cookie as the sole credential. It holds no tokens, runs no OAuth authorization-code / token-exchange / refresh flow, and sends `credentials: 'include'` on every Management API request. Authorization state is derived from the server, not from token expiry.

### Guardrails (from AB#61845)

- Applies to the **backoffice client only**.
- **OIDC must remain** for API users, Swagger UI, Postman (server-side, untouched).
- **Delivery API** must not be affected at all.
- This is under the "Next Major (Breaking changes)" epic, so breaking the client-side auth API surface is acceptable in v19 — but we limit gratuitous breakage where cheap (see §"Public API surface").

## Scope

**In scope:**
- Rewrite `UmbAuthContext` (`src/packages/core/auth/auth.context.ts`) and the boot orchestration in `src/apps/app/app-auth.controller.ts`.
- Session check + expiry (AB#70020): `GET current-user/configuration` as the boot probe; extend its response model with the session expiry.
- Retain and rewire the session-timeout modal against the cookie expiry.
- Cookie-based sign-out.
- Verify external login providers still round-trip (AB#70073).

**Out of scope (explicit):**
- Unifying the login screen into the backoffice client. This is a deliberate follow-up: the login screen needs certain configuration upfront (pre-auth) that requires additional plumbing. For this POC the login UI stays server-rendered at `/umbraco/login`.
- Any change to OIDC for API users / Swagger / Postman, or to the Delivery API.
- Removing the client-side provider-selection modal (`umb-app-auth-modal`, `umb-auth-view`, `auth-provider-default`). It becomes redundant once auth is delegated to the login page, but is left in place to keep this diff focused; removal rides with the login-unification follow-up.

## Design decisions (locked)

1. **Session check = `GET current-user/configuration`, called first.** 200 → logged in; 401 → not logged in. The response already carries `keepUserLoggedIn`; it is extended with the session expiry. No separate `GET current-user` probe, no new dedicated `/session` endpoint. Side-effect-free: the backoffice cookie is `SlidingExpiration = false` and only renews when `KeepUserLoggedIn` is set (or via the periodic security-stamp revalidation), so probing configuration does not extend a non-keep-logged-in session.
2. **Commit fully** — rip out the client-side OIDC token machinery (PKCE, `/authorize`, `/token`, refresh, popup, `oauth_complete` handling, the `expiresIn * 4` model), rather than leaving it dormant behind a flag.
3. **Login seam at the server login page.** On "not authorized", the client does a full redirect to `/umbraco/login`. After `POST /login` sets the cookie, the login page redirects **straight back to the backoffice root** (not to `oauth_complete`).

## Backend changes (small)

### 1. Session expiry on the current-user configuration endpoint (AB#70020)

- Extend `CurrentUserConfigurationResponseModel` with the session expiry. Preferred shape: an absolute `timeoutUtc` (ISO-8601), so the client computes remaining time against its own clock without a drift-prone "seconds" snapshot. (Alternative `secondsUntilTimeout` is acceptable if it matches an existing convention — decide during implementation; default to absolute.)
- Source the value from the `Constants.Security.TicketExpiresClaimType` claim, which `ConfigureBackOfficeCookieOptions.OnValidatePrincipal` already writes on every validated request.
- `ConfigurationCurrentUserController` runs behind `BackOfficeAccess`, so an unauthenticated request already yields 401 — the client's "not logged in" signal. No new endpoint.
- Update `Umbraco.Cms.Api.Management/OpenApi.json` (substantive changes only; no IDE reformatting).

### 2. Login page post-login redirect

- After a successful `POST /login` (and 2FA where applicable), the server login page redirects to the backoffice root instead of `oauth_complete?code=…`. This is the only login-page change in scope.

## Frontend design

### Session model

Replace the token-derived session:

```ts
// before (token-shaped)
interface UmbAuthSession { accessTokenExpiresAt: number; expiresAt: number; }
const TOKEN_EXPIRY_MULTIPLIER = 4;

// after (cookie-shaped)
interface UmbAuthSession {
  /** Absolute session expiry (from current-user/configuration). null when keepUserLoggedIn. */
  expiresAt: number | null;
  keepUserLoggedIn: boolean;
}
```

No access-token concept, no refresh timers, no `expiresIn * 4`.

### Boot flow (`setInitialState` + `app-auth.controller`)

1. `UmbAuthContext.setInitialState()` calls `GET current-user/configuration`.
2. **200** → authorized: store `expiresAt` (null if `keepUserLoggedIn`) and `keepUserLoggedIn`; `isAuthorized = true`.
3. **401** → not authorized: `getIsAuthorized()` returns false. `UmbAppAuthController` stores the current path and does `location.href = /umbraco/login?returnPath=…` — a full redirect. This replaces `makeAuthorizationRequest` / the provider-selection modal at boot.
4. Cross-tab: keep `BroadcastChannel('umb:auth')` but reduce to `signedOut` / `sessionCleared` only. Session-sharing / token messages (`authorized`, `sessionUpdate`, `sessionRequest`/`sessionResponse`) are removed — any tab can cheaply re-probe the cookie via configuration.

### Runtime — timeout modal + 401 handling

- **Timeout modal is retained** as a save-your-work guard. Rationale: with `keepUserLoggedIn = false` the cookie has a fixed, non-sliding expiry, so the session ends mid-work regardless of activity; the countdown modal warns the user in time to save rather than silently losing unsaved edits on the next 401.
  - `keepUserLoggedIn = false` → run the countdown to `expiresAt`; show the modal near expiry.
  - `keepUserLoggedIn = true` → dormant (never show the overlay).
  - **Constraint to verify:** a fixed-expiry (`keepUserLoggedIn = false`) cookie has no server-side "extend" path, so the modal is *warn-and-re-login*, not a silent renew. Confirm there is no lingering "keep me logged in" affordance that would call a now-removed `/token` refresh.
- **401 interceptor:** today it attempts a token refresh then replays the request. New behaviour: a 401 means the cookie is gone/expired → no refresh is possible → trigger the same redirect-to-login as the boot 401 path (after storing the return path). The timeout modal is the friendly early warning; the 401 redirect is the hard backstop.

### Sign-out

`signOut()` keeps the `GET /security/back-office/signout` redirect (it clears the cookies server-side via `OnSigningOut`) and drops the OIDC `revokeToken()` call. Broadcast `signedOut` so other tabs follow to the logout page.

### Public API surface

- **Deleted:** `makeAuthorizationRequest`, `completeAuthorizationRequest`, `validateToken`, `makeRefreshTokenRequest`, PKCE/popup plumbing, `oauth_complete` route handling, and use of `src/external/openid` from the auth path.
- **Kept but deprecated as no-op shims** (to limit blast radius across extensions and internal consumers such as `preview.element.ts` and the SignalR `server-event.context.ts`):
  - `getLatestToken()` → returns `''` (the cookie carries auth; no bearer needed).
  - `getOpenApiConfiguration()` → `{ base, credentials: 'include' }`, no `token`.
  - Add `@deprecated` JSDoc + `UmbDeprecation` runtime warning (see `docs/deprecation.md`), scheduled for removal in a later major.

## Task mapping

| Task | Covered by |
|------|-----------|
| AB#70020 (/session check) | Backend §1 (config expiry) + Frontend boot flow steps 1–2 |
| AB#70019 (finish Kenn's FE) | Session model, boot flow, timeout/401, sign-out, public API surface |
| AB#70073 (external providers) | Verification (below) — no client code change; external login is handled entirely on the server login page |

## External login (AB#70073)

External providers are initiated and completed on the server login page (challenge → external callback → cookie → redirect back). The client rewrite does not touch that path, so this is a verification task: confirm a provider (e.g. Google) round-trips to a set cookie and lands back in the authorized backoffice. Account linking/unlinking (`linkLogin` / `unlinkLogin` in current-user) is unaffected and stays.

## Verification

Auth is sensitive — every scenario is browser-tested manually, plus `tsc` + eslint before each commit (per repo practice).

1. Fresh login (username/password) → lands in backoffice.
2. Reload while authorized → no re-login (cookie honoured, configuration 200s).
3. `keepUserLoggedIn = false`: session reaches expiry → timeout modal warns → logout.
4. `keepUserLoggedIn = true`: no timeout modal; session persists across activity.
5. 401 mid-session (e.g. cookie cleared server-side) → redirect to `/umbraco/login` with return path.
6. Sign-out → cookie cleared, other tabs follow.
7. External provider (e.g. Google) → round-trips and lands authorized.
8. 2FA-enabled user → `verify-2fa` path still works.

## Open items to resolve during implementation

- `timeoutUtc` (absolute, preferred) vs `secondsUntilTimeout` on the config model — pick to match existing conventions; default absolute.
- Exact return-path contract between the client redirect (`?returnPath=`) and the login page's post-login redirect back to the backoffice.
- Confirm no remaining consumer relies on the deleted methods beyond the shimmed `getLatestToken` / `getOpenApiConfiguration`.
