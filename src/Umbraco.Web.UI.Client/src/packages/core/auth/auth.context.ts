import { UMB_AUTH_CONTEXT } from './auth.context.token.js';
import { UmbAuthSessionTimeoutController } from './controllers/auth-session-timeout.controller.js';
import type { UmbOpenApiConfiguration } from './models/openApiConfiguration.js';
import type { ManifestAuthProvider } from './auth-provider.extension.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbApiInterceptorController, UMB_AUTH_SIGNALER_CONTEXT } from '@umbraco-cms/backoffice/resources';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { ReplaySubject, Subject, distinctUntilChanged, auditTime, map } from '@umbraco-cms/backoffice/external/rxjs';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApiClient, umbHttpClient } from '@umbraco-cms/backoffice/http-client';
import { isTestEnvironment, UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * Client routes worth returning to after a login: the back office proper (`section/:sectionName` —
 * see `UMB_SECTION_PATH_PATTERN`) and the other routes behind the auth guard. Everything else is a
 * boot route (install, logout, error, auth-callback) that either renders without a session — so
 * returning there would just show the login screen again — or isn't a destination at all.
 *
 * Deliberately an allowlist: omitting a returnable route only costs a deep link (the user lands on
 * the back office root), while omitting a session-less one loops the login.
 */
const RETURNABLE_ROUTES = ['section', 'upgrade', 'preview'];

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
	 * Observable that emits when the auth context is initialized.
	 * @deprecated The auth context is initialized on creation and this emits immediately. Scheduled for removal in Umbraco 21.
	 * @remark It will only emit once and then complete itself.
	 */
	get isInitialized(): Observable<void> {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.isInitialized',
			removeInVersion: '21.0.0',
			solution: 'The auth context is ready once constructed; remove the dependency on this signal.',
		}).warn();
		return this.#isInitialized.asObservable();
	}

	/**
	 * Observable that emits true if the user is authorized, otherwise false.
	 * It will only emit when the authorization state changes.
	 */
	readonly isAuthorized = this.#session.asObservable().pipe(
		map((session) => this.#isBypassed || !!session),
		distinctUntilChanged(),
	);

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
					break;
				case 'signedOut':
					this.#session.setValue(undefined);
					// Redirect to logout page — cookies already cleared by the tab that initiated sign-out
					location.href = this.#postLogoutRedirectUri;
					break;
			}
		};

		if (!isTestEnvironment()) {
			new UmbAuthSessionTimeoutController(this);
		}

		// The auth context is ready once constructed — provider discovery no longer waits on a signal.
		this.#isInitialized.next();
		this.#isInitialized.complete();

		// When an HTTP interceptor is active it registers an UmbAuthSignalerContext on the host.
		// Consume it to keep authorization state in sync and to react to timeout requests.
		this.consumeContext(UMB_AUTH_SIGNALER_CONTEXT, (signaler) => {
			this.observe(this.isAuthorized, (isAuthorized) => signaler?.setAuthorized(isAuthorized ?? false));
			// React to timeout requests from the interceptor
			this.observe(signaler?.timeoutRequest, () => {
				// Only time out if the user is currently authorized
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
	 * other provider is challenged via the cookie external-login endpoint.
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
		// Preserve where the user was, but only when it is somewhere worth returning to.
		const deepLink = this.#isReturnableRoute() ? window.location.pathname + window.location.search : undefined;

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
	 * @deprecated No-op — the server sets the auth cookie directly, there is no code exchange. Always returns null. Scheduled for removal in Umbraco 21.
	 * @returns {Promise<null>} Always null.
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
	getIsAuthorized(): boolean {
		return this.#isBypassed || !!this.#session.getValue();
	}

	/**
	 * Sets the initial state of the auth flow.
	 * This must be called before any other auth methods are called.
	 * It establishes if the user is authorized or not, and sets the session state accordingly.
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
	 * This is the canonical, reusable way to keep a session alive: it pings the server keep-alive
	 * endpoint.
	 * @returns {boolean} True if the session was renewed, otherwise false.
	 */
	async keepAlive(): Promise<boolean> {
		const response = await this.#fetchWithCookie(this.#keepAliveEndpoint, 'POST');
		if (!response?.ok) {
			return false;
		}

		// The cookie's expiry was renewed server-side; re-read it and refresh the local session.
		return this.#establishSessionFromServer();
	}

	/**
	 * Calls an auth endpoint with the session cookie, bypassing the generated (intercepted) client:
	 * a 401 here is the expected "no session" answer, whereas the API interceptor would queue the
	 * request for re-authentication and stall the caller. Resolves undefined if the request failed.
	 * @param {string} url The endpoint to call.
	 * @param {'GET' | 'POST'} method The HTTP method to use.
	 * @returns {Promise<Response | undefined>} The response, or undefined if the request threw.
	 */
	async #fetchWithCookie(url: string, method: 'GET' | 'POST'): Promise<Response | undefined> {
		try {
			return await fetch(url, {
				method,
				credentials: 'include',
				// Don't follow the server's redirect to /login; a non-ok response is the answer.
				redirect: 'manual',
				headers: { Accept: 'application/json' },
			});
		} catch {
			return undefined;
		}
	}

	/**
	 * Probes current-user/configuration and applies the resulting session locally (and broadcasts to
	 * peer tabs). Returns true when authorized, false otherwise.
	 * @returns {Promise<boolean>} True if the session was established, otherwise false.
	 */
	async #establishSessionFromServer(): Promise<boolean> {
		const response = await this.#fetchWithCookie(
			`${this.#serverUrl}/umbraco/management/api/v1/user/current/configuration`,
			'GET',
		);

		try {
			if (!response?.ok) {
				this.#session.setValue(undefined);
				return false;
			}

			const data = await response.json();
			const issuedAt = Math.floor(Date.now() / 1000);
			const expiresIn = data.timeoutUtc
				? Math.max(0, Math.floor(new Date(data.timeoutUtc).getTime() / 1000) - issuedAt)
				: 60 * 60;
			this.#setSessionLocally(expiresIn, issuedAt);

			// Tell other tabs a session is (re)established, so they can update their local state too.
			this.#channel.postMessage({ type: 'authorized', expiresIn, issuedAt });
			return true;
		} catch {
			this.#session.setValue(undefined);
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
	 * @deprecated Cookie auth verifies the session with the server on boot and per request, so a local expiry check is redundant. Use {@link getIsAuthorized} or observe {@link session$}. Scheduled for removal in Umbraco 21.
	 * @returns {boolean} True if the session has not expired.
	 */
	isSessionValid(): boolean {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.isSessionValid()',
			removeInVersion: '21.0.0',
			solution: 'Use getIsAuthorized() or observe session$ instead.',
		}).warn();
		const session = this.#session.getValue();
		return !!session && session.expiresAt > Math.floor(Date.now() / 1000);
	}

	/**
	 * Clears the in-memory session state and broadcasts to other tabs.
	 * @deprecated Cookie auth stores no client-side token, and clearing local state without the server sign-out leaves the auth cookie intact (the next request re-authenticates). Use {@link signOut} to log out. Scheduled for removal in Umbraco 21.
	 * @memberof UmbAuthContext
	 */
	clearTokenStorage() {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.clearTokenStorage()',
			removeInVersion: '21.0.0',
			solution: 'Use signOut() to log out.',
		}).warn();
		this.#session.setValue(undefined);
		this.#channel.postMessage({ type: 'sessionCleared' });
	}

	/**
	 * Handles the case where the user has timed out, i.e. the token has expired.
	 * This will clear the token storage and set the user as unauthorized.
	 * @memberof UmbAuthContext
	 */
	timeOut() {
		this.#session.setValue(undefined);
		this.#isTimeout.next();
	}

	/**
	 * Signs the user out by clearing the local session and redirecting to the server sign-out
	 * endpoint, which clears the authentication cookie.
	 * @memberof UmbAuthContext
	 */
	async signOut(): Promise<void> {
		// Clear local state directly (not clearTokenStorage) to skip its deprecation warning; signedOut covers other tabs.
		this.#session.setValue(undefined);
		this.#channel.postMessage({ type: 'signedOut' });

		// The server sign-out endpoint clears the auth cookie, then redirects to the client logout landing
		// (derived server-side from BackOfficeHost).
		location.href = `${this.#serverUrl}/umbraco/management/api/v1/security/back-office/signout`;
	}

	/**
	 * Get the server url to the Management API.
	 * @deprecated Consume {@link UMB_SERVER_CONTEXT} and use its `getServerUrl()` — the canonical source for the server URL. Scheduled for removal in Umbraco 21.
	 * @memberof UmbAuthContext
	 * @returns {string} The server url to the Management API
	 */
	getServerUrl(): string {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.getServerUrl()',
			removeInVersion: '21.0.0',
			solution: 'Consume UMB_SERVER_CONTEXT and use its getServerUrl() instead.',
		}).warn();
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
			// Deprecated (removal v21): cookie auth carries no client token, so this callback is a no-op.
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
	 * Sets the auth context as initialized.
	 * @deprecated The auth context is ready once constructed and no longer gates provider discovery on this signal; it is now a no-op. Scheduled for removal in Umbraco 21.
	 */
	setInitialized() {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.setInitialized()',
			removeInVersion: '21.0.0',
			solution: 'Remove the call; the auth context is ready once constructed.',
		}).warn();
	}

	/**
	 * Gets all registered auth providers.
	 * @deprecated Query the extension registry directly: `extensionsRegistry.byType('authProvider')`. Scheduled for removal in Umbraco 21.
	 * @param {UmbBackofficeExtensionRegistry} extensionsRegistry The extensions registry to get auth providers from.
	 * @returns {Observable<ManifestAuthProvider[]>} An observable that emits the registered auth providers.
	 */
	getAuthProviders(extensionsRegistry: UmbBackofficeExtensionRegistry): Observable<ManifestAuthProvider[]> {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.getAuthProviders()',
			removeInVersion: '21.0.0',
			solution: "Query the extension registry directly with extensionsRegistry.byType('authProvider').",
		}).warn();
		return extensionsRegistry.byType<'authProvider', ManifestAuthProvider>('authProvider');
	}

	/**
	 * Whether the current location is somewhere a user should be returned to after logging in.
	 * @returns {boolean} True for the back office's section routes and the other guarded routes.
	 */
	#isReturnableRoute(): boolean {
		// Strip the back-office path so this holds whether the client is served at "/" or "/umbraco",
		// then match on the top-level route segment.
		const { pathname } = window.location;
		const route = pathname.startsWith(this.#backofficePath)
			? pathname.slice(this.#backofficePath.length)
			: pathname;

		return RETURNABLE_ROUTES.includes(route.split('/').filter(Boolean)[0]);
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
