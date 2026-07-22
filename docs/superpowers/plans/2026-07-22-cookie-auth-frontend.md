# Cookie-based Authentication — Frontend POC Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> **Note on who executes:** Jacob is driving this implementation by hand (auth is sensitive — human touch). This plan is his working guide. Claude stays on the sideline: reviewing diffs, checking the deletion strands no consumers, and browser-verifying alongside. Claude does not edit the auth files unless asked.

**Goal:** Replace Kenn's `COOKIE_AUTH_BYPASS` stub with a real, purely cookie-based backoffice auth client — the httpOnly login cookie is the sole credential; no client-side tokens, OAuth flow, or refresh.

**Architecture:** On boot the client probes `GET /user/current/configuration` (200 = authorized, 401 = redirect to `/umbraco/login`). The response carries `keepUserLoggedIn` and the session expiry, which drive a retained save-your-work timeout modal. The client's OIDC token machinery (PKCE, `/authorize`, `/token`, refresh, `oauth_complete`) is removed; `getLatestToken`/`getOpenApiConfiguration` survive as deprecated no-op shims for external extensions.

**Tech Stack:** .NET 10 / ASP.NET Core (Management API), TypeScript + Lit (backoffice client, `@umbraco-cms/backoffice`), OpenIddict (server-side, untouched), hey-api generated client.

## Global Constraints

- Base branch: `v19/feature/management-api-cookie-auth` (continuing on Kenn's branch directly). Commit signing is automatic — never bypass.
- Backoffice client only. **OIDC must remain** server-side for API users / Swagger / Postman. **Delivery API untouched.**
- v19 is a new major under the "Next Major (Breaking changes)" epic — breaking the client-side auth API surface is allowed; still keep `getLatestToken`/`getOpenApiConfiguration` as deprecated shims to avoid needless extension breakage.
- Deprecations use BOTH `@deprecated` JSDoc AND `UmbDeprecation` runtime warning (see `src/Umbraco.Web.UI.Client/docs/deprecation.md`).
- Run `tsc` + eslint before every frontend commit. Auth flows are browser-verified manually (see Task 7 matrix) — that is the real gate.
- `OpenApi.json` changes: substantive only, no IDE reformatting.
- Never call `validateToken()` per API request (revokes the reference token → ID2019). Not applicable after this work, but do not reintroduce.

## File structure

**Backend (Management API):**
- `src/Umbraco.Cms.Api.Management/ViewModels/User/Current/CurrentUserConfigurationResponseModel.cs` — add `TimeoutUtc`.
- `src/Umbraco.Cms.Api.Management/Factories/UserPresentationFactory.cs` — populate `TimeoutUtc` from the ticket-expiry claim.
- `src/Umbraco.Cms.Api.Management/OpenApi.json` — regenerate the two touched fragments.
- `tests/Umbraco.Tests.Integration/ManagementApi/Factories/UserPresentationFactoryTests.cs` — cover the new field.

**Frontend (`src/Umbraco.Web.UI.Client/src`):**
- `packages/core/auth/auth.context.ts` — session model + boot probe + strip token machinery + shims (the core change).
- `apps/app/app-auth.controller.ts` — not-authorized/timeout → redirect to `/umbraco/login`.
- `apps/app/app.element.ts` — remove the `oauth_complete` route and its `#setAuthStatus` guard.
- `packages/core/auth/controllers/auth-session-timeout.controller.ts` — schedule off cookie expiry; dormant when `keepUserLoggedIn`; drop client refresh.
- `packages/core/auth/modals/umb-auth-timeout-modal.element.ts` — remove the "stay logged in / continue" affordance.
- `packages/core/auth/umb-auth-client.ts` + `src/external/openid/*` usage + `UmbAppOauthElement` — deleted/orphaned by the above.

**Out of scope:** the separate login bundle (`@umbraco-cms/login`, served via `UmbracoLogin/Index.cshtml`). It already renders `<umb-auth return-url="@backOfficePath">` — i.e. it returns to the backoffice root, not `oauth_complete`. The login→backoffice handoff therefore already works in cookie mode (Kenn's POC proved it). Login-into-client unification is a deliberate follow-up.

---

## Task 1: Backend — session expiry on the current-user configuration endpoint (AB#70020)

**Files:**
- Modify: `src/Umbraco.Cms.Api.Management/ViewModels/User/Current/CurrentUserConfigurationResponseModel.cs`
- Modify: `src/Umbraco.Cms.Api.Management/Factories/UserPresentationFactory.cs`
- Modify: `src/Umbraco.Cms.Api.Management/OpenApi.json`
- Test: `tests/Umbraco.Tests.Integration/ManagementApi/Factories/UserPresentationFactoryTests.cs`

**Interfaces:**
- Produces: `CurrentUserConfigurationResponseModel.TimeoutUtc: DateTimeOffset?` — absolute session expiry; `null` when `KeepUserLoggedIn` is true (dormant) or the ticket-expiry claim is absent. The frontend boot probe (Task 3) reads `timeoutUtc` + `keepUserLoggedIn`.

- [ ] **Step 1: Add the property to the response model**

In `CurrentUserConfigurationResponseModel.cs`, add:

```csharp
/// <summary>
/// Gets or sets the absolute UTC time at which the current session expires,
/// or <c>null</c> when the session has no fixed expiry (e.g. <see cref="KeepUserLoggedIn"/> is set).
/// </summary>
public DateTimeOffset? TimeoutUtc { get; set; }
```

- [ ] **Step 2: Write the failing integration test**

In `UserPresentationFactoryTests.cs`, add a test asserting that with `KeepUserLoggedIn = false` and a ticket-expiry claim present on the current principal, `TimeoutUtc` is populated from that claim; and with `KeepUserLoggedIn = true`, `TimeoutUtc` is `null`. Mirror the existing test's harness for building the factory and faking `IHttpContextAccessor.HttpContext.User` with a `Claim(Constants.Security.TicketExpiresClaimType, expiry.ToString("o"), ClaimValueTypes.DateTime)`.

```csharp
[Test]
public async Task CreateCurrentUserConfiguration_populates_TimeoutUtc_from_ticket_claim_when_not_keep_logged_in()
{
    var expiry = DateTimeOffset.UtcNow.AddMinutes(20);
    // arrange: SecuritySettings.KeepUserLoggedIn = false; principal has TicketExpiresClaimType = expiry ("o")
    var model = await Factory.CreateCurrentUserConfigurationModelAsync();
    Assert.That(model.TimeoutUtc, Is.EqualTo(expiry).Within(TimeSpan.FromSeconds(1)));
}

[Test]
public async Task CreateCurrentUserConfiguration_leaves_TimeoutUtc_null_when_keep_logged_in()
{
    // arrange: SecuritySettings.KeepUserLoggedIn = true
    var model = await Factory.CreateCurrentUserConfigurationModelAsync();
    Assert.That(model.TimeoutUtc, Is.Null);
}
```

- [ ] **Step 3: Run the test, verify it fails**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~UserPresentationFactoryTests" -v minimal`
Expected: the two new tests FAIL (`TimeoutUtc` always null / not populated).

- [ ] **Step 4: Populate `TimeoutUtc` in the factory**

`UserPresentationFactory` does not currently take `IHttpContextAccessor`. Add it to the primary constructor (v19 permits the breaking change; if you prefer strict back-compat, apply the obsolete-constructor pattern from `CLAUDE.md §5.1` and resolve the accessor via `StaticServiceProvider`). Store `_httpContextAccessor`, then in `CreateCurrentUserConfigurationModelAsync`:

```csharp
DateTimeOffset? timeoutUtc = null;
if (_securitySettings.KeepUserLoggedIn is false)
{
    var expires = _httpContextAccessor.HttpContext?.User
        .FindFirst(Constants.Security.TicketExpiresClaimType)?.Value;
    if (string.IsNullOrEmpty(expires) is false
        && DateTimeOffset.TryParse(expires, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset parsed))
    {
        timeoutUtc = parsed;
    }
}

var model = new CurrentUserConfigurationResponseModel
{
    KeepUserLoggedIn = _securitySettings.KeepUserLoggedIn,
    TimeoutUtc = timeoutUtc,
    PasswordConfiguration = _passwordConfigurationPresentationFactory.CreatePasswordConfigurationResponseModel(),
    AllowChangePassword = _externalLoginProviders.HasDenyLocalLogin() is false,
    AllowTwoFactor = _externalLoginProviders.HasDenyLocalLogin() is false,
};
```

(Check `src/Umbraco.Core/Security/ClaimsPrincipalExtensions.cs` first — it already reads `TicketExpiresClaimType`; reuse an existing helper if one fits rather than re-parsing.)

- [ ] **Step 5: Run the test, verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~UserPresentationFactoryTests" -v minimal`
Expected: PASS.

- [ ] **Step 6: Regenerate OpenApi.json**

Run the instance, open the management swagger.json, and copy the updated `CurrentUserConfigurationResponseModel` schema into `src/Umbraco.Cms.Api.Management/OpenApi.json` (that fragment only — no reformatting elsewhere). Regenerate the client types (Task 3 depends on `timeoutUtc` existing in `types.gen.ts`).

- [ ] **Step 7: Commit**

```bash
git add src/Umbraco.Cms.Api.Management tests/Umbraco.Tests.Integration
git commit -m "feat(api): expose session TimeoutUtc on current-user configuration (AB#70020)"
```

---

## Task 2: Verify the login→backoffice handoff (no code change)

**Files:** none (verification only).

The client will stop doing the OIDC dance, so confirm the existing seam holds: `/umbraco/login` sets the cookie and returns to `return-url` (the backoffice root), and the backoffice then recognises the cookie.

- [ ] **Step 1: Confirm the return-url**

Confirm `src/Umbraco.Cms.StaticAssets/umbraco/UmbracoLogin/Index.cshtml` still renders `<umb-auth ... return-url="@backOfficePath">` (backoffice root, not `oauth_complete`). No change needed if so.

- [ ] **Step 2: Note the assumption**

Record (in the PR description) that the separate login bundle POSTs credentials and navigates to `return-url` without requiring the backoffice to complete `/authorize`+`/token`. Kenn's POC (`COOKIE_AUTH_BYPASS`) already boots authorized purely on the cookie, so this is proven. If a future change to the login bundle is required, it belongs to the login-unification follow-up, not this POC.

---

## Task 3: Frontend — cookie session model + boot probe (AB#70019, AB#70020)

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/src/packages/core/auth/auth.context.ts`

**Interfaces:**
- Produces: `UmbAuthSession { expiresAt: number | null; keepUserLoggedIn: boolean }` (absolute unix seconds; `null` = dormant). `getIsAuthorized(): boolean`, `setInitialState(): Promise<void>`, `session$`, `isAuthorized` observables — consumed by Tasks 4 and 5.

- [ ] **Step 1: Replace the session interface and remove the multiplier**

```ts
export interface UmbAuthSession {
	/** Absolute session expiry (unix seconds) from current-user/configuration; null when keepUserLoggedIn. */
	expiresAt: number | null;
	keepUserLoggedIn: boolean;
}
```

Delete `const TOKEN_EXPIRY_MULTIPLIER = 4;` and the `COOKIE_AUTH_BYPASS` block.

- [ ] **Step 2: Rewrite `getIsAuthorized`**

```ts
getIsAuthorized(): boolean {
	if (this.#isBypassed) {
		this.#isAuthorized.setValue(true);
		return true;
	}
	const isAuthorized = !!this.#session.getValue();
	this.#isAuthorized.setValue(isAuthorized);
	return isAuthorized;
}
```

- [ ] **Step 3: Rewrite `setInitialState` to probe the configuration endpoint**

Use a raw `fetch` (not the configured client) so the 401 interceptor/retry-queue does not fire during boot.

```ts
async setInitialState(): Promise<void> {
	if (this.#isBypassed) return;
	if (this.#session.getValue()) return;

	try {
		const res = await fetch(
			`${this.#serverUrl}/umbraco/management/api/v1/user/current/configuration`,
			{ method: 'GET', credentials: 'include', headers: { Accept: 'application/json' } },
		);
		if (!res.ok) {
			// 401 (or other) → treat as unauthenticated; app-auth.controller redirects to login.
			this.#session.setValue(undefined);
			this.#isAuthorized.setValue(false);
			return;
		}
		const config = (await res.json()) as { keepUserLoggedIn?: boolean; timeoutUtc?: string | null };
		const keepUserLoggedIn = config.keepUserLoggedIn ?? false;
		const expiresAt =
			keepUserLoggedIn || !config.timeoutUtc ? null : Math.floor(new Date(config.timeoutUtc).getTime() / 1000);
		this.#session.setValue({ expiresAt, keepUserLoggedIn });
		this.#isAuthorized.setValue(true);
	} catch {
		this.#session.setValue(undefined);
		this.#isAuthorized.setValue(false);
	}
}
```

Update the readonly `keepUserLoggedIn` field: derive it from the session where consumers need it (or keep the constructor value as the pre-auth default and prefer `session.keepUserLoggedIn` at runtime). Keep the constructor param for now; the config response is authoritative post-auth.

- [ ] **Step 4: Simplify cross-tab handling**

In the `BroadcastChannel('umb:auth')` `onmessage`, keep only `signedOut` and `sessionCleared`. Remove the `authorized`, `sessionUpdate`, `sessionRequest`/`sessionResponse` cases and the `#requestSessionFromPeers` helper (no token/session to share — each tab re-probes the cookie).

- [ ] **Step 5: Verify (tsc + eslint + browser)**

Run: `npm --prefix src/Umbraco.Web.UI.Client run build:tsc` (or the project's `tsc`/`lint` scripts).
Browser: log in via `/umbraco/login`; confirm the backoffice loads authorized and a reload does **not** force re-login (check the network tab shows a 200 on `user/current/configuration`).

- [ ] **Step 6: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src/packages/core/auth/auth.context.ts
git commit -m "feat(auth): derive session from current-user/configuration cookie probe (AB#70020)"
```

---

## Task 4: Frontend — redirect to /umbraco/login on not-authorized & timeout (AB#70019)

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/src/apps/app/app-auth.controller.ts`
- Modify: `src/Umbraco.Web.UI.Client/src/apps/app/app.element.ts`

**Interfaces:**
- Consumes: `getIsAuthorized()`, `timeoutSignal` (Task 3). Produces: a full-page redirect to `/umbraco/login?returnPath=…`.

- [ ] **Step 1: Replace the auth-request flow with a login redirect**

In `app-auth.controller.ts`, replace `makeAuthorizationRequest`/`#showLoginModal` usage with a redirect helper. `isAuthorized()` becomes:

```ts
async isAuthorized(): Promise<boolean> {
	await this.#retrievedModal.catch(() => undefined);
	if (!this.#authContext) throw new Error('[Fatal] Auth context is not available');
	if (this.#authContext.getIsAuthorized()) return true;
	this.#redirectToLogin();
	return false;
}

#redirectToLogin() {
	let currentUrl = window.location.href;
	const params = new URLSearchParams(window.location.search);
	if (params.has('returnPath')) currentUrl = decodeURIComponent(params.get('returnPath') || currentUrl);
	setStoredPath(currentUrl);
	const serverUrl = this.#authContext!.getServerUrl();
	const returnPath = encodeURIComponent(new URL(currentUrl).pathname + new URL(currentUrl).search);
	location.href = `${serverUrl}/umbraco/login?returnPath=${returnPath}`;
}
```

Point the `timeoutSignal` observer at `#redirectToLogin()` too (replacing `this.makeAuthorizationRequest('timedOut')`). Remove now-unused imports (`UMB_MODAL_APP_AUTH`, `umbOpenModal`, `UmbPersistentModalDialogElement`, `firstValueFrom`, `umbExtensionsRegistry`, `UmbUserLoginState`).

- [ ] **Step 2: Remove the `oauth_complete` route**

In `app.element.ts`, delete the entire `{ path: 'oauth_complete', component: UmbAppOauthElement, setup: … }` route object and the `UmbAppOauthElement` import. In `#setAuthStatus`, delete the `window.opener && pathname === '/oauth_complete'` early-return guard (the whole `pathname`/`pathWithoutBasePath` guard block) so it always calls `setInitialState()`.

- [ ] **Step 3: Verify (tsc + eslint + browser)**

Browser: from a logged-out state, hit any backoffice URL → confirm a full redirect to `/umbraco/login` carrying `returnPath`; after login, confirm you land back on the originally requested path.

- [ ] **Step 4: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src/apps/app/app-auth.controller.ts src/Umbraco.Web.UI.Client/src/apps/app/app.element.ts
git commit -m "feat(auth): redirect to /umbraco/login when unauthenticated; drop oauth_complete route (AB#70019)"
```

---

## Task 5: Frontend — timeout controller & modal against the cookie expiry (AB#70019)

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/src/packages/core/auth/controllers/auth-session-timeout.controller.ts`
- Modify: `src/Umbraco.Web.UI.Client/src/packages/core/auth/modals/umb-auth-timeout-modal.element.ts`

**Interfaces:**
- Consumes: `session$` (`{ expiresAt, keepUserLoggedIn }`), `timeoutSignal`, `signOut()`, `timeOut()`.

- [ ] **Step 1: Schedule only on a fixed expiry; drop client refresh**

In the controller's `session$` observer, schedule only when `session.expiresAt` is a number (i.e. `keepUserLoggedIn === false`); when `expiresAt` is `null`, clear any scheduled check and leave the modal dormant:

```ts
this.observe(host.session$, (session) => {
	this.#clearScheduledCheck();
	if (session && session.expiresAt !== null) {
		this.#closeTimeoutModal();
		this.#scheduleCheck(session.expiresAt);
	}
}, '_sessionState');
```

In `#onSessionExpiring`, remove the `keepUserLoggedIn` auto-refresh branch and the `#tryValidateToken` method entirely. When the buffer is reached: if fully expired → `this.#host.timeOut()`, else `this.#openTimeoutModal(secondsRemaining)`. The modal's `onContinue` is removed (Step 2), so `#openTimeoutModal` passes only `onLogout` and `onExpired`.

- [ ] **Step 2: Remove the "stay logged in" affordance from the modal**

In `umb-auth-timeout-modal.element.ts`, delete `#handleConfirm` and the `#confirm` "continue" `<uui-button>`. Keep the countdown, the logout button, and the `onExpired` submit. The modal is now a save-your-work warning: log out now, or let it expire (→ `timeOut()` → redirect to login via Task 4). Drop `onContinue` from `UmbModalAuthTimeoutConfig` (`umb-auth-timeout-modal.token.ts`).

- [ ] **Step 3: Verify (tsc + eslint + browser)**

Browser, `KeepUserLoggedIn=false` (short `Global:TimeOut`, e.g. 1–2 min): confirm the modal appears before expiry, "Log out" signs out, and letting it lapse redirects to `/umbraco/login`. Then `KeepUserLoggedIn=true`: confirm the modal never appears and the session persists across activity.

- [ ] **Step 4: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src/packages/core/auth/controllers/auth-session-timeout.controller.ts src/Umbraco.Web.UI.Client/src/packages/core/auth/modals/umb-auth-timeout-modal.element.ts src/Umbraco.Web.UI.Client/src/packages/core/auth/modals/umb-auth-timeout-modal.token.ts
git commit -m "feat(auth): drive timeout modal from cookie expiry; dormant when keepUserLoggedIn (AB#70019)"
```

---

## Task 6: Frontend — strip token machinery; sign-out; no-op shims (AB#70019)

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/src/packages/core/auth/auth.context.ts`
- Delete/orphan: `src/Umbraco.Web.UI.Client/src/packages/core/auth/umb-auth-client.ts`, `src/external/openid/*` usage, `UmbAppOauthElement`.

**Interfaces:**
- Produces: `getLatestToken(): Promise<string>` → `''`; `getOpenApiConfiguration()` → `{ base, credentials: 'include' }` (both deprecated). `signOut(): Promise<void>` (cookie clear via `/signout`, no revoke). `configureClient` sets `credentials: 'include'`, no `auth` callback.

- [ ] **Step 1: Delete the OIDC methods**

Remove from `auth.context.ts`: `makeAuthorizationRequest`, `completeAuthorizationRequest`, `validateToken`, `makeRefreshTokenRequest`, `#performRefresh`, `#ensureTokenReady`, `#isAccessTokenValid`, `#requestCodeVerifierFromOpener`, `#setSessionLocally`/`#updateSession` (token-timing helpers), the popup fields (`#authWindowProxy`, `#popupCleanup`), `#sessionDead`, `#inSessionUpdateCallback`, PKCE endpoints, and the `#client = new UmbAuthClient(...)` wiring. Delete `umb-auth-client.ts` and remove the `src/external/openid` import from the auth path. Remove `UmbAppOauthElement` if now unreferenced.

- [ ] **Step 2: Convert token accessors to deprecated shims (BOTH methods)**

Deprecate `getLatestToken` AND `getOpenApiConfiguration` for external consumers — each with a `@deprecated` JSDoc tag AND a `UmbDeprecation` runtime warning (check `docs/deprecation.md` first; confirm the exact `removeInVersion` against the repo's deprecation policy — N+2 from v19 → `21.0.0`).

```ts
/** @deprecated Cookie auth carries credentials automatically; no bearer token exists. Scheduled for removal in Umbraco 21. */
async getLatestToken(): Promise<string> {
	new UmbDeprecation({ deprecated: 'getLatestToken', solution: 'Requests are authenticated by the httpOnly session cookie automatically (credentials: "include").', removeInVersion: '21.0.0' }).warn();
	return '';
}

/** @deprecated Cookie auth carries credentials automatically; construct requests with `credentials: "include"`. Scheduled for removal in Umbraco 21. */
getOpenApiConfiguration(): UmbOpenApiConfiguration {
	new UmbDeprecation({ deprecated: 'getOpenApiConfiguration', solution: 'Use credentials: "include" directly; the httpOnly session cookie authenticates requests.', removeInVersion: '21.0.0' }).warn();
	return { base: this.#serverUrl, credentials: 'include', token: undefined };
}
```

Confirm `UmbOpenApiConfiguration.token` is optional; make it optional if not. `configureClient` sets `{ baseUrl, credentials: 'include' }` and no `auth` callback. Note: both methods have zero internal callers, so the runtime warning only ever fires for external extensions — exactly the audience we want to nudge off them.

- [ ] **Step 3: Simplify `signOut`**

```ts
async signOut(): Promise<void> {
	this.#session.setValue(undefined);
	this.#isAuthorized.setValue(false);
	this.#channel.postMessage({ type: 'signedOut' });
	const postLogoutRedirectUri = new URL(this.#postLogoutRedirectUri, window.location.origin);
	const endSessionEndpoint = `${this.#serverUrl}/umbraco/management/api/v1/security/back-office/signout`;
	const postLogoutLocation = new URL(endSessionEndpoint);
	postLogoutLocation.searchParams.set('post_logout_redirect_uri', postLogoutRedirectUri.href);
	location.href = postLogoutLocation.href;
}
```

(Drop the `#client.revokeToken()` call.) Keep `linkLogin`/`unlinkLogin`, but replace their `Authorization: Bearer ${await this.getLatestToken()}` headers with plain `credentials: 'include'` (no bearer needed).

- [ ] **Step 4: Verify (tsc + eslint + browser)**

`tsc`/lint must be clean (this proves no internal consumer relied on a deleted method — recall `getLatestToken`/`getOpenApiConfiguration` have no internal callers). Browser: sign out → confirm the cookie is cleared (dev-tools Application tab) and you land on the login page; confirm normal API calls still succeed (cookie carries auth, no `Authorization` header on requests).

- [ ] **Step 5: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src
git commit -m "refactor(auth): remove client OIDC token machinery; cookie-only with deprecated shims (AB#70019)"
```

---

## Task 7: Verify external login providers + full auth matrix (AB#70073)

**Files:** none (verification; fixes land back in the relevant task above if a defect is found).

- [ ] **Step 1: External provider round-trip**

Configure an external provider (e.g. Google) in the test instance. From logged-out, hit the backoffice → redirected to `/umbraco/login` → choose the provider → complete the external challenge → confirm the external callback sets the cookie and lands you authorized in the backoffice.

- [ ] **Step 2: Run the full browser matrix (auth is sensitive — verify each)**

- [ ] Fresh username/password login → backoffice.
- [ ] Reload while authorized → no re-login (200 on `user/current/configuration`).
- [ ] `KeepUserLoggedIn=false`: expiry → timeout modal warns → logout / lapse → redirect to login.
- [ ] `KeepUserLoggedIn=true`: no timeout modal; session persists across activity.
- [ ] 401 mid-session (clear the cookie server-side) → redirect to `/umbraco/login` with `returnPath`.
- [ ] Sign-out → cookie cleared, other open tabs follow to the login page.
- [ ] 2FA-enabled user → `verify-2fa` path still completes login.
- [ ] External provider → round-trips and lands authorized.

- [ ] **Step 3: Update the work items**

Set AB#70019 / AB#70020 / AB#70073 to Done; note verification results and the deferred login-unification follow-up in AB#70018.

---

## Self-review notes

- **Spec coverage:** §2 backend expiry → Task 1; §2 login-page redirect → Task 2 (no-op, verified); §3 session model → Task 3; §4 boot flow → Tasks 3+4; §5 timeout/401 → Tasks 4+5; §6 sign-out → Task 6; §7 public API (deleted + shims) → Task 6; §8 external login → Task 7; §9 verification → Tasks 3–7. All covered.
- **Assumption flagged (Task 2):** the separate login bundle returns to the backoffice root without needing client-side `/authorize`+`/token`. De-risked by Kenn's working POC; a login-bundle change, if ever needed, is the deferred unification follow-up — surface immediately if Step-1 browser verification of Task 3/4 shows login not completing.
- **Type consistency:** `UmbAuthSession { expiresAt: number | null; keepUserLoggedIn: boolean }` used identically in Tasks 3 and 5; `TimeoutUtc` (backend) → `timeoutUtc` (client JSON) → `expiresAt` (client session) chain is explicit in Task 3 Step 3.
