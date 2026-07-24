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
	 * Initiates external login for the given provider by opening a popup pointed at the server's
	 * cookie-based external login challenge endpoint. The server redirects to the provider, then
	 * back through its callback (which sets the auth cookie), then to the client's auth-callback
	 * lander, which broadcasts `authorized` and closes the popup. No PKCE/OIDC state is involved —
	 * the httpOnly auth cookie set by the server is the sole credential.
	 * @param {string} provider The provider to log in with.
	 * @param {ManifestAuthProvider} manifest The manifest for the registered provider, used for the popup target/features.
	 */
	startExternalLogin(provider: string, manifest?: ManifestAuthProvider): void {
		const challengeUrl = new URL(`${this.#serverUrl}/umbraco/management/api/v1/security/back-office/external-login`);
		challengeUrl.searchParams.set('provider', provider);

		// Preserve where the user was so context carries through the flow. Skip a bare backoffice
		// root — the server already defaults there, so a returnUrl would just be noise.
		const returnPath = window.location.pathname + window.location.search;
		if (returnPath !== this.#backofficePath) {
			challengeUrl.searchParams.set('returnUrl', returnPath);
		}

		const popupTarget = manifest?.meta?.behavior?.popupTarget ?? 'umbracoAuthPopup';
		const popupFeatures =
			manifest?.meta?.behavior?.popupFeatures ??
			'width=600,height=600,menubar=no,location=no,resizable=yes,scrollbars=yes,status=no,toolbar=no';

		window.open(challengeUrl.href, popupTarget, popupFeatures);
	}

	/**
	 * Initiates local (username/password) login by opening a popup pointed at the server-rendered
	 * login app. On success the login app sets the auth cookie and redirects to the `ReturnUrl` we
	 * pass — the client's auth-callback lander — which broadcasts `authorized` and closes the popup,
	 * so the main window keeps any unsaved work (same rationale as {@link startExternalLogin}).
	 *
	 * Local login cannot reuse the external-login challenge endpoint: that issues a `ChallengeResult`
	 * for a named authentication scheme, and local login has no such scheme.
	 * @param {ManifestAuthProvider} manifest The manifest for the built-in provider, used for the popup target/features.
	 */
	startLocalLogin(manifest?: ManifestAuthProvider): void {
		const loginUrl = new URL(`${this.#serverUrl}/umbraco/login`);

		// Return the popup to our auth-callback lander after login. Pass a LOCAL, RELATIVE path: the
		// server login controller validates ReturnUrl with Url.IsLocalUrl (which rejects absolute /
		// cross-origin URLs), and the login app then resolves it against its configured back-office-host
		// — so in a split-origin setup (dev server / Umbraco Cloud) the popup still lands on the client
		// host, not the backend's. `document.baseURI` gives the path under the client's base.
		loginUrl.searchParams.set('ReturnUrl', new URL('auth-callback', document.baseURI).pathname);

		const popupTarget = manifest?.meta?.behavior?.popupTarget ?? 'umbracoAuthPopup';
		const popupFeatures =
			manifest?.meta?.behavior?.popupFeatures ??
			'width=600,height=600,menubar=no,location=no,resizable=yes,scrollbars=yes,status=no,toolbar=no';

		window.open(loginUrl.href, popupTarget, popupFeatures);
	}

	/**
	 * Initiates login for the single available provider on a cold boot (no session yet), navigating
	 * full-page instead of opening a popup — there is no in-progress work to preserve, and skipping the
	 * modal + popup gives a direct login (matching the pre-cookie-auth behaviour). Local login goes to
	 * the server login app; an external provider goes straight to its challenge endpoint.
	 * @param {ManifestAuthProvider} manifest The single registered provider to initiate.
	 */
	autoInitiateLogin(manifest: ManifestAuthProvider): void {
		if (manifest.forProviderName.toLowerCase() === 'umbraco') {
			window.location.href = `${this.#serverUrl}/umbraco/login`;
			return;
		}

		const challengeUrl = new URL(`${this.#serverUrl}/umbraco/management/api/v1/security/back-office/external-login`);
		challengeUrl.searchParams.set('provider', manifest.forProviderName);
		window.location.href = challengeUrl.href;
	}

	/**
	 * Checks if the user is authorized. If Authorization is bypassed, the user is always authorized.
	 * @returns True if the user is authorized, otherwise false.
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
		return '[redacted]';
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
		// multiplier) no longer applies. TODO (V19 cleanup): collapse UmbAuthSession to one expiresAt.
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
