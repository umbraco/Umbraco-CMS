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
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApiClient, umbHttpClient } from '@umbraco-cms/backoffice/http-client';
import { isTestEnvironment, UmbDeprecation } from '@umbraco-cms/backoffice/utils';

export interface UmbAuthSession {
	/**
	 * @deprecated Cookie auth has a single, server-owned expiry, so this is now identical to
	 * {@link expiresAt}. Use `expiresAt`. Scheduled for removal in Umbraco 21.
	 */
	accessTokenExpiresAt: number;
	/** When the session (auth cookie) expires. Used for the timeout UI. */
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

	// Session timing — in-memory only, no localStorage
	#session = new UmbObjectState<UmbAuthSession | undefined>(undefined);
	readonly session$ = this.#session.asObservable();

	// Cross-tab coordination
	#channel: BroadcastChannel;

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
	#externalLoginEndpoint;
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

		this.#postLogoutRedirectUri = this.getPostLogoutRedirectUrl();

		this.#linkEndpoint = `${serverUrl}/umbraco/management/api/v1/security/back-office/link-login`;
		this.#linkKeyEndpoint = `${serverUrl}/umbraco/management/api/v1/security/back-office/link-login-key`;
		this.#unlinkEndpoint = `${serverUrl}/umbraco/management/api/v1/security/back-office/unlink-login`;
		this.#keepAliveEndpoint = `${serverUrl}/umbraco/management/api/v1/security/back-office/keep-alive`;
		this.#externalLoginEndpoint = `${serverUrl}/umbraco/management/api/v1/security/back-office/external-login`;

		// Set up cross-tab coordination via BroadcastChannel
		this.#channel = new BroadcastChannel('umb:auth');
		this.#channel.onmessage = (evt: MessageEvent) => {
			switch (evt.data?.type) {
				case 'authorized': {
					// Apply locally — the sender already broadcast to all tabs.
					this.#setSessionLocally(evt.data.expiresIn, evt.data.issuedAt);
					break;
				}
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
			// React to timeout requests from the interceptor. A 401 while we were never authorized
			// (e.g. the cold-boot session probe) means "not logged in", not a session timeout — raising
			// the timeout signal there would wrongly show the "session timed out" state on a fresh login.
			this.observe(signaler?.timeoutRequest, () => {
				if (this.getIsAuthorized()) {
					this.timeOut();
				}
			});
		});
	}

	override destroy(): void {
		super.destroy();
		this.#channel.close();
	}

	/**
	 * Initiates login for the given provider.
	 *
	 * The built-in "Umbraco" provider is local username/password login (the server login app); any
	 * other provider is challenged via the cookie external-login endpoint. With `redirect` the flow
	 * navigates full-page (cold-boot single-provider login — nothing to preserve); otherwise it opens
	 * a popup so the current view keeps any unsaved work and adopts the session via the auth-callback
	 * lander's `authorized` broadcast. No PKCE/OIDC state is involved — the httpOnly cookie the server
	 * sets is the sole credential.
	 * @param {string} identityProvider The provider to log in with. Default 'Umbraco' (local login).
	 * @param {boolean} redirect Navigate full-page instead of opening a popup.
	 * @param {string} _usernameHint Ignored (cookie auth has no username hint).
	 * @param {ManifestAuthProvider} manifest The registered provider's manifest, used for the popup target/features.
	 */
	async makeAuthorizationRequest(
		identityProvider = 'Umbraco',
		redirect?: boolean,
		_usernameHint?: string,
		manifest?: ManifestAuthProvider,
	): Promise<void> {
		// Preserve where the user was so login returns them there. Skip a bare backoffice root — the
		// server defaults there. The server re-validates it with Url.IsLocalUrl (a relative path).
		const returnPath = window.location.pathname + window.location.search;
		const deepLink = returnPath === this.#backofficePath ? undefined : returnPath;

		let target: URL;
		if (identityProvider.toLowerCase() === 'umbraco') {
			target = new URL(`${this.#serverUrl}/umbraco/login`);
			// A popup must land on the auth-callback lander (it broadcasts `authorized` and closes the
			// popup); a full-page redirect returns to the deep link instead.
			const returnUrl = redirect ? deepLink : new URL('auth-callback', document.baseURI).pathname;
			if (returnUrl) {
				target.searchParams.set('ReturnUrl', returnUrl);
			}
		} else {
			// External login always routes through the server callback to the auth-callback lander;
			// carry the deep link so the lander's full-page fallback can return there.
			const challengeUrl = new URL(this.#externalLoginEndpoint);
			challengeUrl.searchParams.set('provider', identityProvider);
			target = challengeUrl;
			if (deepLink) {
				target.searchParams.set('returnUrl', deepLink);
			}
		}

		if (redirect) {
			window.location.href = target.href;
			return;
		}

		const popupTarget = manifest?.meta?.behavior?.popupTarget ?? 'umbracoAuthPopup';
		const popupFeatures =
			manifest?.meta?.behavior?.popupFeatures ??
			'width=600,height=600,menubar=no,location=no,resizable=yes,scrollbars=yes,status=no,toolbar=no';

		window.open(target.href, popupTarget, popupFeatures);
	}

	/**
	 * Completes the login flow.
	 * This is called on the oauth_complete page to exchange the authorization code for tokens.
	 * @returns The token response timing, or null if no authorization was pending.
	 * @deprecated No-op — the server sets the auth cookie directly, there is no code exchange. Always returns null. Scheduled for removal in Umbraco 21.
	 */
	async completeAuthorizationRequest(): Promise<null> {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.completeAuthorizationRequest()',
			removeInVersion: '21.0.0',
			solution: 'There is no authorization code exchange with cookie auth; remove the call.',
		}).warn();
		return null;
	}

	/**
	 * Checks if the user is authorized. If Authorization is bypassed, the user is always authorized.
	 * @returns {boolean} True if the user is authorized, otherwise false.
	 */
	getIsAuthorized() {
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
			// Probe the current-user configuration with a direct fetch, NOT the generated (intercepted)
			// client. A 401 here is the expected "no session" answer to the boot probe; routing it
			// through the API interceptor would queue this request for re-authentication (so the promise
			// never resolves and boot hangs) and raise a spurious timeout signal. We handle the response
			// manually and set the session state accordingly.
			const response = await fetch(`${this.#serverUrl}/umbraco/management/api/v1/user/current/configuration`, {
				method: 'GET',
				credentials: 'include',
				redirect: 'manual',
				headers: { Accept: 'application/json' },
			});

			if (!response.ok) {
				this.#session.setValue(undefined);
				this.#isAuthorized.setValue(false);
				return false;
			}

			const data = await response.json();
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
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.getLatestToken()',
			removeInVersion: '21.0.0',
			solution:
				'Back-office auth is cookie-based and carries no client token. Use configureClient()/getOpenApiConfiguration(), or set credentials: "include" on fetch calls.',
		}).warn();
		return '[redacted]';
	}

	/**
	 * Forces a token refresh against the server (calls `/token`) and returns true if successful.
	 * Use this when you need to unconditionally refresh — e.g. session timeout keep-alive.
	 * For per-request token handling, prefer {@link configureClient} which skips the network
	 * call when the access token is still valid.
	 * Uses Web Locks to deduplicate concurrent refresh requests across tabs.
	 * @deprecated Cookie auth has no token to validate — returns {@link getIsAuthorized}. Use {@link keepAlive} to extend the session. Scheduled for removal in Umbraco 21.
	 * @memberof UmbAuthContext
	 * @returns {Promise<boolean>} True if the refresh succeeded, otherwise false
	 */
	async validateToken(): Promise<boolean> {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.validateToken()',
			removeInVersion: '21.0.0',
			solution: 'Use getIsAuthorized() or keepAlive() instead.',
		}).warn();
		return this.getIsAuthorized();
	}

	/**
	 * Attempts to refresh the token using Web Locks to prevent concurrent refresh requests.
	 * @deprecated Cookie auth has no refresh token — delegates to {@link keepAlive}, which extends the session by renewing the cookie. Scheduled for removal in Umbraco 21.
	 * @returns {Promise<boolean>} True if the refresh was successful, otherwise false.
	 */
	async makeRefreshTokenRequest(): Promise<boolean> {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.makeRefreshTokenRequest()',
			removeInVersion: '21.0.0',
			solution: 'Use keepAlive() to extend the session.',
		}).warn();
		return this.keepAlive();
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
	 * Signs the user out by clearing the local session and redirecting to the server sign-out
	 * endpoint, which clears the authentication cookie.
	 * @memberof UmbAuthContext
	 */
	async signOut(): Promise<void> {
		// Clear local state (don't call clearTokenStorage — signedOut covers other tabs)
		this.#session.setValue(undefined);
		this.#isAuthorized.setValue(false);
		this.#channel.postMessage({ type: 'signedOut' });

		// Navigate to the server sign-out endpoint: it clears the auth cookie and then redirects to the
		// client logout landing (derived server-side from BackOfficeHost), which resets the SPA state.
		location.href = `${this.#serverUrl}/umbraco/management/api/v1/security/back-office/signout`;
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
			// Deprecated (removal v21): cookie auth carries no client token — the auth cookie rides along
			// via credentials: 'include'. Kept as a shim so existing `await config.token()` callers don't
			// throw; returns the redacted placeholder.
			token: async () => {
				new UmbDeprecation({
					deprecated: 'UmbOpenApiConfiguration.token',
					removeInVersion: '21.0.0',
					solution: 'The auth cookie is sent automatically with credentials: "include"; remove the token() call.',
				}).warn();
				return '[redacted]';
			},
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
			// Don't follow 302 redirects to /login — the auth interceptor handles 401s and replays requests after re-authentication.
			redirect: 'manual',
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
	 * @param {UmbBackofficeExtensionRegistry} extensionsRegistry The extensions registry to get auth providers from.
	 * @returns {Observable<ManifestAuthProvider[]>} An observable that emits the registered auth providers.
	 */
	getAuthProviders(extensionsRegistry: UmbBackofficeExtensionRegistry): Observable<ManifestAuthProvider[]> {
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
		const request = new Request(this.#unlinkEndpoint, {
			method: 'POST',
			credentials: 'include',
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
	 * Sets the in-memory session state (does not broadcast — callers that establish a session on
	 * behalf of other tabs, e.g. {@link #establishSessionFromServer}, broadcast separately).
	 * @param {number} expiresIn The number of seconds until the session expires.
	 * @param {number} issuedAt The timestamp when the session was issued.
	 */
	#setSessionLocally(expiresIn: number, issuedAt: number) {
		// Cookie auth: the session has a single, server-owned expiry (the auth cookie's), so both
		// timestamps are the same — the historical access-vs-refresh token split (and its ×4
		// multiplier) no longer applies. TODO (V21): drop the deprecated accessTokenExpiresAt.
		const expiresAt = issuedAt + expiresIn;
		this.#session.setValue({ accessTokenExpiresAt: expiresAt, expiresAt });
		this.#isAuthorized.setValue(true);
	}

	async #makeLinkTokenRequest(provider: string) {
		const request = await fetch(`${this.#linkKeyEndpoint}?provider=${provider}`, {
			credentials: 'include',
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
