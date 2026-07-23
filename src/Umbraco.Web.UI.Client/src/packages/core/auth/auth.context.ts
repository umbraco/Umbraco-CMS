import { UmbAuthClient } from './umb-auth-client.js';
import type { UmbAuthClientEndpoints, UmbTokenEndpointResponse } from './umb-auth-client.js';
import { UMB_AUTH_CONTEXT } from './auth.context.token.js';
import { UmbAuthSessionTimeoutController } from './controllers/auth-session-timeout.controller.js';
import type { UmbOpenApiConfiguration } from './models/openApiConfiguration.js';
import type { ManifestAuthProvider } from './auth-provider.extension.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbApiInterceptorController, UMB_AUTH_SIGNALER_CONTEXT } from '@umbraco-cms/backoffice/resources';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import {
	ReplaySubject,
	Subject,
	switchMap,
	distinctUntilChanged,
	auditTime,
} from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApiClient, umbHttpClient } from '@umbraco-cms/backoffice/http-client';
import { isTestEnvironment } from '@umbraco-cms/backoffice/utils';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * TEMPORARY TESTING BYPASS — NOT FOR PRODUCTION.
 *
 * When true, the backoffice skips the OpenID Connect authorization-code / token-exchange
 * flow entirely and trusts the authentication cookie(s) issued by the server login at
 * `/umbraco/login`. Those cookies are already sent on every Management API request
 * (`credentials: 'include'`), so the client only needs to consider itself authorized.
 *
 * This deliberately ignores access-token expiry, refresh, and "seconds until logout" UX.
 * Flip back to `false` (or delete the gated branches) to restore the real OIDC flow.
 */
const COOKIE_AUTH_BYPASS = true;

export interface UmbAuthSession {
	/** When the access token expires (issuedAt + expiresIn). Used to decide when to refresh. */
	accessTokenExpiresAt: number;
	/** When the full session expires (issuedAt + expiresIn * MULTIPLIER). Used for timeout UI. */
	expiresAt: number;
}

export class UmbAuthContext extends UmbContextBase {
	#isAuthorized = new UmbBooleanState<boolean>(false);
	// Timeout is different from `isAuthorized` because it can occur repeatedly
	#isTimeout = new Subject<void>();
	#isInitialized = new ReplaySubject<void>(1);
	#isBypassed;
	#serverUrl;
	#backofficePath;

	// Auth client (replaces appauth library)
	#client: UmbAuthClient;

	// Session timing — in-memory only, no localStorage
	#session = new UmbObjectState<UmbAuthSession | undefined>(undefined);
	readonly session$ = this.#session.asObservable();

	// Set when a refresh was definitively rejected by the server (e.g. invalid_grant).
	// Distinguishes "no session yet" from "session is dead" so concurrent and subsequent
	// API requests don't each fire their own doomed /token call. Cleared when a new
	// session is established (login, peer tab, completed re-authentication).
	#sessionDead = false;

	// True only during the synchronous #updateSession() call inside the lock callback.
	// Prevents re-entrant /token calls when session$ observers fire synchronously
	// (e.g. keepUserLoggedIn=true with short expiresIn triggers #onSessionExpiring
	// from inside the lock, capturing sessionBefore = newSession so the guard can't help).
	#inSessionUpdateCallback = false;

	// Cross-tab coordination
	#channel: BroadcastChannel;

	// Popup management
	#authWindowProxy?: WindowProxy | null;
	#popupCleanup?: () => void;

	// Track clients that have been configured to prevent duplicate interceptor binding
	#configuredClients = new WeakSet();

	// Lazily initialised on the first configureClient() call. Owns the singleton
	// UmbAuthSignalerContext provided on the host (`<umb-app>`), so we MUST share
	// one instance across every client we configure — instantiating a new controller
	// per call would re-provide the signaler and stack listeners.
	#interceptorController?: UmbApiInterceptorController;

	// Endpoint URLs
	#linkEndpoint;
	#linkKeyEndpoint;
	#unlinkEndpoint;
	#keepAliveEndpoint;
	#postLogoutRedirectUri;

	/**
	 * Observable that emits true when the auth context is initialized.
	 * It will only emit once and then complete itself.
	 */
	readonly isInitialized = this.#isInitialized.asObservable();

	/**
	 * Observable that emits true if the user is authorized, otherwise false.
	 * It will only emit when the authorization state changes.
	 */
	readonly isAuthorized = this.#isAuthorized.asObservable().pipe(distinctUntilChanged());

	/**
	 * Observable that acts as a signal and emits when the user has timed out, i.e. the token has expired.
	 * This can be used to show a timeout message to the user.
	 * It will emit once per second, so it can be used to trigger UI updates or other actions when the user has timed out.
	 */
	readonly timeoutSignal = this.#isTimeout.asObservable().pipe(
		// Audit the timeout signal to ensure that it waits for 1s before allowing another emission, which prevents rapid firing of the signal.
		// This is useful to prevent the UI from being flooded with timeout events.
		auditTime(1000),
	);

	/**
	 * Whether the server is configured to keep users logged in by auto-refreshing before session expiry.
	 * Provided by the backend via the `keep-user-logged-in` attribute on `<umb-app>`.
	 */
	readonly keepUserLoggedIn: boolean;

	constructor(
		host: UmbControllerHost,
		serverUrl: string,
		backofficePath: string,
		isBypassed: boolean,
		keepUserLoggedIn = false,
	) {
		super(host, UMB_AUTH_CONTEXT);
		this.#isBypassed = isBypassed;
		this.#serverUrl = serverUrl;
		this.#backofficePath = backofficePath;
		this.keepUserLoggedIn = keepUserLoggedIn;

		const redirectUri = this.getRedirectUrl();
		this.#postLogoutRedirectUri = this.getPostLogoutRedirectUrl();

		const endpoints: UmbAuthClientEndpoints = {
			authorizationEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/authorize`,
			tokenEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/token`,
			revocationEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/revoke`,
			linkEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/link-login`,
			linkKeyEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/link-login-key`,
			unlinkEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/unlink-login`,
		};

		this.#linkEndpoint = endpoints.linkEndpoint;
		this.#linkKeyEndpoint = endpoints.linkKeyEndpoint;
		this.#unlinkEndpoint = endpoints.unlinkEndpoint;
		this.#keepAliveEndpoint = `${serverUrl}/umbraco/management/api/v1/security/back-office/keep-alive`;

		this.#client = new UmbAuthClient(endpoints, redirectUri);

		// Set up cross-tab coordination via BroadcastChannel
		this.#channel = new BroadcastChannel('umb:auth');
		this.#channel.onmessage = (evt: MessageEvent) => {
			switch (evt.data?.type) {
				case 'authorized': {
					// Apply locally — do NOT call #updateSession which would re-broadcast.
					this.#setSessionLocally(evt.data.expiresIn, evt.data.issuedAt);
					break;
				}
				case 'sessionUpdate':
					// Peer broadcast already-computed timestamps, so set the session
					// directly. We still go through the `#inSessionUpdateCallback` guard
					// so observers triggered re-entrantly skip a redundant /token call.
					this.#sessionDead = false;
					this.#inSessionUpdateCallback = true;
					try {
						this.#session.setValue({
							accessTokenExpiresAt: evt.data.accessTokenExpiresAt,
							expiresAt: evt.data.expiresAt,
						});
						this.#isAuthorized.setValue(true);
					} finally {
						this.#inSessionUpdateCallback = false;
					}
					break;
				case 'sessionCleared':
					this.#session.setValue(undefined);
					this.#isAuthorized.setValue(false);
					break;
				case 'signedOut':
					this.#session.setValue(undefined);
					this.#isAuthorized.setValue(false);
					// Redirect to logout page — cookies already cleared by the tab that initiated sign-out
					location.href = this.#postLogoutRedirectUri;
					break;
				case 'sessionRequest': {
					// Another tab is asking for the current session state (e.g. new tab opening).
					// Only share the session if it is still valid — an expired session would cause
					// the recipient (e.g. a popup) to believe it is already authorized and skip
					// the authorization code exchange.
					if (this.isSessionValid()) {
						this.#channel.postMessage({ type: 'sessionResponse', session: this.#session.getValue()! });
					}
					break;
				}
			}
		};

		if (!isTestEnvironment()) {
			// Start the session timeout controller
			new UmbAuthSessionTimeoutController(this);
		}

		// When an HTTP interceptor is active it registers an UmbAuthSignalerContext on the host.
		// Consume it to keep authorization state in sync and to react to timeout requests.
		this.consumeContext(UMB_AUTH_SIGNALER_CONTEXT, (signaler) => {
			// Keep the signaler's authorization state in sync with ours
			this.observe(this.isAuthorized, (isAuthorized) => signaler?.setAuthorized(isAuthorized ?? false));
			// React to timeout requests from the interceptor
			this.observe(signaler?.timeoutRequest, () => this.timeOut());
		});
	}

	override destroy(): void {
		// Tear down any in-flight popup auth flow so its window-level message listener
		// and the closed-poll interval don't leak past this context's lifetime.
		this.#popupCleanup?.();
		super.destroy();
		this.#channel.close();
	}

	/**
	 * Initiates the login flow.
	 * @param identityProvider The provider to use for login. Default is 'Umbraco'.
	 * @param redirect If true, the user will be redirected to the login page.
	 * @param usernameHint The username hint to use for login.
	 * @param manifest The manifest for the registered provider.
	 */
	async makeAuthorizationRequest(
		identityProvider = 'Umbraco',
		redirect?: boolean,
		usernameHint?: string,
		manifest?: ManifestAuthProvider,
	): Promise<void> {
		const redirectUrl = await this.#client.buildAuthorizationUrl(identityProvider, usernameHint);

		if (redirect) {
			// For redirect flows, persist PKCE state in sessionStorage (survives same-tab navigation)
			sessionStorage.setItem(
				'umb:pkce',
				JSON.stringify({
					codeVerifier: this.#client.codeVerifier,
					state: this.#client.state,
				}),
			);
			location.href = redirectUrl;
			return;
		}

		const popupTarget = manifest?.meta?.behavior?.popupTarget ?? 'umbracoAuthPopup';
		const popupFeatures =
			manifest?.meta?.behavior?.popupFeatures ??
			'width=600,height=600,menubar=no,location=no,resizable=yes,scrollbars=yes,status=no,toolbar=no';

		// Clean up any pending popup flow before starting a new one
		this.#popupCleanup?.();

		if (!this.#authWindowProxy || this.#authWindowProxy.closed) {
			this.#authWindowProxy = window.open(redirectUrl, popupTarget, popupFeatures);
		} else {
			// Popup still open — navigate to the new URL (always different due to PKCE state)
			this.#authWindowProxy = window.open(redirectUrl, popupTarget);
			this.#authWindowProxy?.focus();
		}

		// Store PKCE state for the popup's postMessage request
		const codeVerifier = this.#client.codeVerifier;
		const state = this.#client.state;

		// Listen for PKCE requests from the popup
		const pkceHandler = (evt: MessageEvent) => {
			if (evt.origin !== window.location.origin) return;
			if (evt.data?.type === 'pkceRequest' && evt.data?.state === state) {
				// Respond with the code_verifier
				this.#authWindowProxy?.postMessage({ type: 'pkceResponse', codeVerifier, state }, window.location.origin);
			}
		};
		window.addEventListener('message', pkceHandler);

		// Wait for the popup to complete via BroadcastChannel.
		// The Promise resolves once cleanup runs — whether triggered by an `authorized`
		// broadcast, the popup being closed/cancelled, a new auth flow superseding this
		// one, or the auth context being destroyed. resolve() is parked inside cleanup
		// so every termination path is observable to the awaiter.
		return new Promise<void>((resolve) => {
			const cleanup = () => {
				clearInterval(closedPoll);
				this.#channel.removeEventListener('message', handler);
				window.removeEventListener('message', pkceHandler);
				this.#popupCleanup = undefined;
				resolve();
			};
			this.#popupCleanup = cleanup;

			const handler = (evt: MessageEvent) => {
				if (evt.data?.type === 'authorized') {
					this.#client.clearPkceState();
					this.#authWindowProxy?.close();
					cleanup();
				}
			};
			this.#channel.addEventListener('message', handler);

			// Poll for popup closed (user cancelled or closed the window)
			const closedPoll = setInterval(() => {
				if (this.#authWindowProxy?.closed) {
					this.#client.clearPkceState();
					cleanup();
				}
			}, 500);
		});
	}

	/**
	 * Completes the login flow.
	 * This is called on the oauth_complete page to exchange the authorization code for tokens.
	 * @returns The token response timing, or null if no authorization was pending.
	 */
	async completeAuthorizationRequest(): Promise<UmbTokenEndpointResponse | null> {
		const searchParams = new URLSearchParams(window.location.search);
		const code = searchParams.get('code');
		const state = searchParams.get('state');

		if (!code) {
			return null;
		}

		// Try to get PKCE state. Check sessionStorage first — it's synchronous and covers
		// the redirect flow (where the same tab navigated to the IDP and back). Only fall
		// back to asking window.opener if sessionStorage didn't have a matching entry.
		// The previous order hung for the full opener-postMessage timeout whenever
		// `oauth_complete` happened to load with a non-OAuth window.opener (which is set
		// for ANY window.open target, not only OAuth popups).
		let codeVerifier: string | undefined;

		const pkceData = sessionStorage.getItem('umb:pkce');
		if (pkceData) {
			try {
				const parsed = JSON.parse(pkceData);
				if (parsed.state === state) {
					codeVerifier = parsed.codeVerifier;
					sessionStorage.removeItem('umb:pkce');
				}
			} catch {
				// Ignore parse errors
				sessionStorage.removeItem('umb:pkce');
			}
		}

		if (!codeVerifier && window.opener) {
			// Popup flow: request code_verifier from parent via postMessage.
			//codeVerifier = await this.#requestCodeVerifierFromOpener(state);
		}

		if (!codeVerifier) {
			console.error('[UmbAuthContext] No code_verifier available for authorization code exchange');
			return null;
		}

		const response = await this.#client.exchangeCode(code, codeVerifier);
		if (!response) {
			return null;
		}

		// Set session locally — use #setSessionLocally (not #updateSession) to avoid
		// broadcasting sessionUpdate AND authorized, which would cause a message storm.
		this.#setSessionLocally(response.expiresIn, response.issuedAt);
		this.#isAuthorized.setValue(true);

		// Broadcast to all tabs that authorization is complete
		// TODO: Is this still needed for v19 auth?
		this.#channel.postMessage({
			type: 'authorized',
			expiresIn: response.expiresIn,
			issuedAt: response.issuedAt,
		});

		return response;
	}

	/**
	 * Checks if the user is authorized. If Authorization is bypassed, the user is always authorized.
	 * @returns True if the user is authorized, otherwise false.
	 */
	getIsAuthorized() {
		if (COOKIE_AUTH_BYPASS) {
			// TEMP: trust the server-issued login cookie; never redirect to /authorize.
			this.#isAuthorized.setValue(true);
			return true;
		}
		if (this.#isBypassed) {
			this.#isAuthorized.setValue(true);
			return true;
		} else {
			const isAuthorized = !!this.#session.getValue();
			this.#isAuthorized.setValue(isAuthorized);
			return isAuthorized;
		}
	}

	/**
	 * Sets the initial state of the auth flow.
	 * First asks existing tabs for their session via BroadcastChannel.
	 * If no peer responds, falls back to a server refresh.
	 * @returns {Promise<void>}
	 */
	async setInitialState(): Promise<void> {
		if (this.#isBypassed) {
			return;
		}

		// If we already have a session, no need to re-initialize
		if (this.#session.getValue()) {
			return;
		}

		await this.#establishSessionFromServer();
	}

	/**
	 * Extends the current back-office session and returns whether it succeeded.
	 *
	 * This is the canonical, reusable way to keep a session alive: it pings the server keep-alive
	 * endpoint (which re-issues the auth cookie with a fresh expiry, regardless of the
	 * `KeepUserLoggedIn` setting), then re-reads the new expiry and refreshes the local session — so
	 * `session$` emits, the timeout is rescheduled, and any open timeout modal is dismissed.
	 *
	 * Call it from anywhere holding the auth context: the session-timeout "Stay logged in" action, a
	 * future activity-based auto-renewer, or an extension that needs to hold a session open during
	 * long-running work. Safe to call repeatedly; it returns `false` (rather than throwing) when the
	 * renewal fails, so callers can decide whether to fall back to sign-out / re-login.
	 * @returns {boolean} True if the session was renewed, otherwise false.
	 */
	async keepAlive(): Promise<boolean> {
		try {
			// The keep-alive endpoint is on BackOfficeController, which is [ApiExplorerSettings(IgnoreApi
			// = true)] — it never appears in OpenApi.json, so there is no generated client method and a
			// direct fetch is the intended approach, not a stopgap. redirect: 'manual' stops an
			// unauthenticated 302 from being followed and mistaken for success.
			const response = await fetch(this.#keepAliveEndpoint, {
				method: 'POST',
				credentials: 'include',
				redirect: 'manual',
				headers: { Accept: 'application/json' },
			});
			if (!response.ok) {
				return false;
			}
		} catch {
			return false;
		}

		// The cookie's expiry was renewed server-side; re-read it and refresh the local session.
		return this.#establishSessionFromServer();
	}

	/**
	 * Probes current-user/configuration and applies the resulting session locally (and broadcasts to
	 * peer tabs). Returns true when authorized, false otherwise.
	 *
	 * redirect: 'manual' is important. When unauthenticated, the back-office cookie middleware's
	 * OnRedirectToLogin issues a 302 to /umbraco/login for non-XHR requests; following it would return
	 * the login HTML as a 200 that we'd mistake for a valid session. Manual mode turns any such
	 * redirect into an opaque, non-ok response (status 0) so it reads as unauthorized — navigation to
	 * the login screen is driven solely by the app auth controller, never by this probe.
	 * @returns {Promise<boolean>} True if the session was established, otherwise false.
	 */
	async #establishSessionFromServer(): Promise<boolean> {
		try {
			// This is a direct call to the backend API to check the current user configuration, so we have to avoid tryExecute() here, otherwise it will trigger a redirect to the login page if the user is not authenticated. Instead, we want to handle the response manually and set the session state accordingly.
			// eslint-disable-next-line local-rules/no-direct-api-import
			const { data, response } = await UserService.getUserCurrentConfiguration({
				credentials: 'include',
				redirect: 'manual',
			});

			if (!response.ok) {
				this.#session.setValue(undefined);
				this.#isAuthorized.setValue(false);
				return false;
			}

			const issuedAt = Math.floor(Date.now() / 1000);
			const expiresIn = data.timeoutUtc
				? Math.max(0, Math.floor(new Date(data.timeoutUtc).getTime() / 1000) - issuedAt)
				: 60 * 60;
			this.#setSessionLocally(expiresIn, issuedAt);

			// Tell other tabs a session is (re)established. A tab that was showing the timeout
			// modal (its own session near expiry) adopts this fresh expiry via the 'authorized'
			// handler and dismisses the modal, instead of waiting for its own countdown to lapse.
			// The handler applies the session locally without re-broadcasting, so no message storm.
			this.#channel.postMessage({ type: 'authorized', expiresIn, issuedAt });
			return true;
		} catch {
			this.#session.setValue(undefined);
			this.#isAuthorized.setValue(false);
			return false;
		}
	}

	/**
	 * Gets the latest token from the Management API.
	 * With cookie auth, this returns '[redacted]' — the real token is in the httpOnly cookie.
	 * If the session has expired, it will attempt a refresh first.
	 * @example <caption>Using the latest token</caption>
	 * ```js
	 *   const token = await authContext.getLatestToken();
	 *   const result = await fetch('https://my-api.com', { headers: { Authorization: `Bearer ${token}` } });
	 * ```
	 * @see {@link configureClient} for automatic token handling with `@hey-api/openapi-ts` clients.
	 * @see {@link getOpenApiConfiguration} for manual fetch calls with cookie-based auth.
	 * @deprecated Use {@link configureClient}, {@link getOpenApiConfiguration}, or remove `"auth"` and set `"include": "credentials"` on fetch calls instead. Scheduled for removal in Umbraco 21.
	 * @memberof UmbAuthContext
	 * @returns {Promise<string>} The latest token from the Management API
	 */
	async getLatestToken(): Promise<string> {
		await this.#ensureTokenReady();
		return '[redacted]';
	}

	/**
	 * Forces a token refresh against the server (calls `/token`) and returns true if successful.
	 * Use this when you need to unconditionally refresh — e.g. session timeout keep-alive.
	 * For per-request token handling, prefer {@link configureClient} which skips the network
	 * call when the access token is still valid.
	 * Uses Web Locks to deduplicate concurrent refresh requests across tabs.
	 * @memberof UmbAuthContext
	 * @returns {Promise<boolean>} True if the refresh succeeded, otherwise false
	 */
	async validateToken(): Promise<boolean> {
		return this.#isBypassed || this.makeRefreshTokenRequest();
	}

	/**
	 * Attempts to refresh the token using Web Locks to prevent concurrent refresh requests.
	 * @returns {Promise<boolean>} True if the refresh was successful, otherwise false.
	 */
	async makeRefreshTokenRequest(): Promise<boolean> {
		// A previous refresh was definitively rejected — retrying cannot succeed
		// until a new session is established.
		if (this.#sessionDead) return false;

		// Fallback for environments without Web Locks (some enterprise/kiosk browsers)
		if (!navigator.locks) {
			console.warn('[UmbAuth] navigator.locks is not available — token refresh coordination disabled.');
			if (this.#isAccessTokenValid()) return true;
			return this.#performRefresh();
		}

		// Capture the session before entering the lock queue. Inside the lock we check
		// if the session object was replaced — that means another tab broadcast a
		// sessionUpdate while we were waiting, so we can skip our own /token call.
		// We compare object references (not expiresAt) because keepUserLoggedIn triggers
		// a proactive refresh *before* the access token expires; an expiresAt-based check
		// would incorrectly skip the refresh when the session is still technically valid.
		const sessionBefore = this.#session.getValue();

		// Guard against re-entrant calls: if session$ fired synchronously from inside
		// a lock callback (via #updateSession → observer → keepUserLoggedIn proactive refresh),
		// sessionBefore would equal the already-updated session so the reference check below
		// can't help. Return true immediately — the lock holder already refreshed.
		if (this.#inSessionUpdateCallback) return true;

		return navigator.locks.request('umb:token-refresh', async () => {
			// A queued caller may have latched the session as dead while we waited for the lock
			if (this.#sessionDead) return false;
			if (this.#session.getValue() !== sessionBefore && this.#isAccessTokenValid()) return true;

			return this.#performRefresh();
		});
	}

	/**
	 * Performs the actual refresh request and applies the result.
	 * A definitive rejection (e.g. `invalid_grant`) marks the session as dead and times the
	 * user out, so the re-authentication flow starts instead of every subsequent API request
	 * firing its own doomed refresh attempt. Transient failures (network errors, 5xx) leave
	 * the session state untouched so a later attempt can retry.
	 * @returns {Promise<boolean>} True if the refresh succeeded, otherwise false.
	 */
	async #performRefresh(): Promise<boolean> {
		const result = await this.#client.refreshToken();
		if (result.response) {
			this.#updateSession(result.response.expiresIn, result.response.issuedAt);
			return true;
		}
		if (result.fatal) {
			this.#sessionDead = true;
			this.timeOut();
		}
		return false;
	}

	/**
	 * Checks if the current session is still valid.
	 * @returns {boolean} True if the session has not expired.
	 */
	isSessionValid(): boolean {
		const session = this.#session.getValue();
		return !!session && session.expiresAt > Math.floor(Date.now() / 1000);
	}

	/**
	 * Local-only check — no network call.
	 * Returns true if the cached access token has not yet reached its expiry timestamp.
	 * Does NOT check the refresh token or server state.
	 */
	#isAccessTokenValid(): boolean {
		const session = this.#session.getValue();
		return !!session && session.accessTokenExpiresAt > Math.floor(Date.now() / 1000);
	}

	/**
	 * Gate for per-request token handling.
	 * - If the access token is expired: calls {@link validateToken} to refresh it (network call).
	 * - If the access token is still valid but another tab holds the `umb:token-refresh` lock:
	 *   waits for that refresh to finish before returning, so the request is sent with the
	 *   latest cookie and not the token that is about to be revoked (prevents ID2019 errors).
	 * - Otherwise: returns immediately with no network call.
	 */
	async #ensureTokenReady(): Promise<void> {
		// The session is dead and re-authentication is already in progress — let the request
		// proceed (and 401) so the interceptor queues it for replay after re-authentication.
		if (this.#sessionDead) return;
		if (!this.#isAccessTokenValid()) {
			await this.validateToken();
			return;
		}
		if (!navigator.locks) return;
		// Always queue behind the refresh lock with a no-op callback. If the lock is
		// free we acquire it immediately and resolve; if a peer tab holds it we wait
		// behind that holder. This avoids a race window where querying the lock state
		// could return "free" microseconds before another tab acquires it.
		await navigator.locks.request('umb:token-refresh', () => Promise.resolve());
	}

	/**
	 * Clears the in-memory session state.
	 * @memberof UmbAuthContext
	 */
	clearTokenStorage() {
		this.#session.setValue(undefined);
		this.#isAuthorized.setValue(false);
		this.#channel.postMessage({ type: 'sessionCleared' });
	}

	/**
	 * Handles the case where the user has timed out, i.e. the token has expired.
	 * This will clear the token storage and set the user as unauthorized.
	 * @memberof UmbAuthContext
	 */
	timeOut() {
		this.#session.setValue(undefined);
		this.#isAuthorized.setValue(false);
		this.#isTimeout.next();
	}

	/**
	 * Signs the user out by revoking tokens and redirecting to the end session endpoint.
	 * @memberof UmbAuthContext
	 */
	async signOut(): Promise<void> {
		// Revoke the token (best-effort)
		await this.#client.revokeToken().catch(() => {});

		// Clear local state (don't call clearTokenStorage — signedOut covers other tabs)
		this.#session.setValue(undefined);
		this.#isAuthorized.setValue(false);
		this.#channel.postMessage({ type: 'signedOut' });

		// Redirect to end session endpoint
		const postLogoutRedirectUri = new URL(this.#postLogoutRedirectUri, window.location.origin);
		const endSessionEndpoint = `${this.#serverUrl}/umbraco/management/api/v1/security/back-office/signout`;
		const postLogoutLocation = new URL(endSessionEndpoint);
		postLogoutLocation.searchParams.set('post_logout_redirect_uri', postLogoutRedirectUri.href);
		location.href = postLogoutLocation.href;
	}

	/**
	 * Get the server url to the Management API.
	 * @memberof UmbAuthContext
	 * @example <caption>Using the server url</caption>
	 * ```js
	 * 	const serverUrl = authContext.getServerUrl();
	 * 	OpenAPI.BASE = serverUrl;
	 * ```
	 * @example <caption></caption>
	 * ```js
	 * 	const config = authContext.getOpenApiConfiguration();
	 * 	const result = await fetch(`${config.base}/umbraco/management/api/v1/my-resource`, {
	 * 		credentials: config.credentials,
	 * 		headers: { Authorization: `Bearer ${await config.token()}` },
	 * 	});
	 * ```
	 * @returns {string} The server url to the Management API
	 */
	getServerUrl(): string {
		return this.#serverUrl;
	}

	/**
	 * Get the default OpenAPI configuration, which is set up to communicate with the Management API
	 * or any other API that uses the same cookie-based authentication.
	 * This is useful if you want to communicate with your own resources generated by the [@hey-api/openapi-ts](https://github.com/hey-api/openapi-ts) library.
	 * @memberof UmbAuthContext
	 * @example <caption>Using the default OpenAPI configuration</caption>
	 * ```js
	 * const defaultOpenApi = authContext.getOpenApiConfiguration();
	 * client.setConfig({
	 *   base: defaultOpenApi.base,
	 *   credentials: defaultOpenApi.credentials,
	 * });
	 * ```
	 * @returns {UmbOpenApiConfiguration} The default OpenAPI configuration
	 */
	getOpenApiConfiguration(): UmbOpenApiConfiguration {
		return {
			base: this.#serverUrl,
			credentials: 'include',
		};
	}

	/**
	 * Configures a `@hey-api/openapi-ts` generated client for authenticated API calls.
	 *
	 * Sets `baseUrl` and `credentials`, and binds the default
	 * response interceptors (401 retry, problem-details error notifications, etc.)
	 * to the client.
	 *
	 * The same auth context owns a single {@link UmbApiInterceptorController} for
	 * the lifetime of the host (`<umb-app>`), so it's safe to call this method for
	 * multiple clients (the core's {@link umbHttpClient} *and* an extension's own
	 * generated client) without registering duplicate auth-signaler contexts.
	 * @example
	 * ```js
	 * const authContext = await this.getContext(UMB_AUTH_CONTEXT);
	 * authContext.configureClient(myClient);
	 * // Now myClient automatically includes auth headers and interceptors
	 * ```
	 * @param {UmbApiClient} client A `@hey-api/openapi-ts` client instance — either {@link umbHttpClient}
	 * or one regenerated by an extension package against its own OpenAPI document. You can see {@link UmbApiClient} for the expected interface.
	 */
	configureClient(client: UmbApiClient): void {
		if (this.#configuredClients.has(client)) return;
		this.#configuredClients.add(client);

		client.setConfig({
			baseUrl: this.#serverUrl,
			credentials: 'include',
			// Cookie auth: the httpOnly auth cookie (sent via credentials: 'include') is the sole
			// credential, so no bearer-token auth callback is needed.
			auth: undefined,
		});

		// Lazy single instance — see #interceptorController field comment. Controller
		// self-registers on the host element via UmbControllerBase, so its lifecycle is
		// managed by the host. `_host` must be a proper UmbControllerHost.
		this.#interceptorController ??= new UmbApiInterceptorController(this._host);
		// Each generated client is structurally identical but TypeScript treats them as
		// distinct generic instantiations. Cast at the boundary; the controller's own
		// signature stays strictly typed against `umbHttpClient`.
		this.#interceptorController.bindDefaultInterceptors(client as unknown as typeof umbHttpClient);
	}

	/**
	 * Sets the auth context as initialized, which means that the auth context is ready to be used.
	 * This is used to let the app context know that the core module is ready, which means that the core auth providers are available.
	 */
	setInitialized() {
		this.#isInitialized.next();
		this.#isInitialized.complete();
	}

	/**
	 * Gets all registered auth providers.
	 * @param extensionsRegistry
	 */
	getAuthProviders(extensionsRegistry: UmbBackofficeExtensionRegistry) {
		return this.#isInitialized.pipe(
			switchMap(() => extensionsRegistry.byType<'authProvider', ManifestAuthProvider>('authProvider')),
		);
	}

	/**
	 * Gets the authorized redirect url.
	 * @returns {string} The redirect url, which is the backoffice path.
	 */
	getRedirectUrl(): string {
		return `${window.location.origin}${this.#backofficePath}`;
	}

	/**
	 * Gets the post logout redirect url.
	 * @returns {string} The post logout redirect url, which is the backoffice path with the logout path appended.
	 */
	getPostLogoutRedirectUrl(): string {
		return `${window.location.origin}${this.#backofficePath}${this.#backofficePath.endsWith('/') ? '' : '/'}logout`;
	}

	/**
	 * Links the current user to the specified provider by redirecting to the link endpoint.
	 * @param {string} provider The provider to link to.
	 */
	async linkLogin(provider: string): Promise<void> {
		const linkKey = await this.#makeLinkTokenRequest(provider);

		const form = document.createElement('form');
		form.method = 'POST';
		form.action = this.#linkEndpoint;
		form.style.display = 'none';

		const providerInput = document.createElement('input');
		providerInput.name = 'provider';
		providerInput.value = provider;
		form.appendChild(providerInput);

		const linkKeyInput = document.createElement('input');
		linkKeyInput.name = 'linkKey';
		linkKeyInput.value = linkKey;
		form.appendChild(linkKeyInput);

		document.body.appendChild(form);
		form.submit();
	}

	/**
	 * Unlinks the current user from the specified provider.
	 * @param {string} loginProvider The provider to unlink from.
	 * @param {string} providerKey The provider key to unlink from.
	 * @returns {Promise<boolean>} True if the unlink was successful, otherwise false.
	 */
	async unlinkLogin(loginProvider: string, providerKey: string): Promise<boolean> {
		const config = this.getOpenApiConfiguration();
		const request = new Request(this.#unlinkEndpoint, {
			method: 'POST',
			credentials: config.credentials ?? 'include',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ loginProvider, providerKey }),
		});

		const result = await fetch(request);

		if (!result.ok) {
			// Wrap the parsed body in a real Error so consumers using `instanceof Error`
			// or expecting a stack trace get sane behaviour. The original problem-details
			// payload is exposed on `.cause` for callers that want the structured fields.
			const detail = await result.json().catch(() => undefined);
			throw new Error(`Failed to unlink login (${result.status} ${result.statusText})`, { cause: detail });
		}

		await this.signOut();

		return true;
	}

	/**
	 * Sets the in-memory session state without broadcasting.
	 * Use when the caller handles broadcasting separately (e.g. completeAuthorizationRequest).
	 *
	 * Sets #inSessionUpdateCallback around the setValue calls to prevent re-entrant /token
	 * requests triggered by session$ observers firing synchronously (e.g. keepUserLoggedIn=true
	 * with a short expiresIn causes #onSessionExpiring to fire immediately).
	 * @param {number} expiresIn The number of seconds until the session expires.
	 * @param {number} issuedAt The timestamp when the session was issued.
	 */
	#setSessionLocally(expiresIn: number, issuedAt: number) {
		// Cookie auth: the session has a single, server-owned expiry (the auth cookie's), so both
		// timestamps are the same — the historical access-vs-refresh token split (and its ×4
		// multiplier) no longer applies. TODO (V19 cleanup): collapse UmbAuthSession to one expiresAt.
		const expiresAt = issuedAt + expiresIn;
		this.#sessionDead = false;
		this.#inSessionUpdateCallback = true;
		try {
			this.#session.setValue({ accessTokenExpiresAt: expiresAt, expiresAt });
			this.#isAuthorized.setValue(true);
		} finally {
			this.#inSessionUpdateCallback = false;
		}
	}

	/**
	 * Updates the in-memory session state and broadcasts to other tabs.
	 * @param {number} expiresIn The number of seconds until the session expires.
	 * @param {number} issuedAt The timestamp when the session was issued.
	 */
	#updateSession(expiresIn: number, issuedAt: number) {
		this.#setSessionLocally(expiresIn, issuedAt);
		const session = this.#session.getValue()!;
		this.#channel.postMessage({
			type: 'sessionUpdate',
			accessTokenExpiresAt: session.accessTokenExpiresAt,
			expiresAt: session.expiresAt,
		});
	}

	async #makeLinkTokenRequest(provider: string) {
		const config = this.getOpenApiConfiguration();
		const request = await fetch(`${this.#linkKeyEndpoint}?provider=${provider}`, {
			credentials: config.credentials ?? 'include',
			headers: {
				'Content-Type': 'application/json',
			},
		});

		if (!request.ok) {
			throw new Error('Failed to link login');
		}

		return request.json();
	}
}
