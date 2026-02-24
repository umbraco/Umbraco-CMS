import { UmbAuthClient } from './umb-auth-client.js';
import type { UmbAuthClientEndpoints, UmbTokenEndpointResponse } from './umb-auth-client.js';
import { UMB_AUTH_CONTEXT } from './auth.context.token.js';
import { UmbAuthSessionTimeoutController } from './controllers/auth-session-timeout.controller.js';
import type { UmbOpenApiConfiguration } from './models/openApiConfiguration.js';
import type { ManifestAuthProvider } from './auth-provider.extension.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbApiInterceptorController } from '@umbraco-cms/backoffice/resources';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import {
	ReplaySubject,
	Subject,
	switchMap,
	distinctUntilChanged,
	throttleTime,
	auditTime,
} from '@umbraco-cms/backoffice/external/rxjs';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { umbHttpClient } from '@umbraco-cms/backoffice/http-client';
import { isTestEnvironment, UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * The multiplier for the token expiry time.
 * In Umbraco, access_tokens live for a quarter of the time of the refresh_token.
 * We multiply by this to get the full session lifetime.
 */
const TOKEN_EXPIRY_MULTIPLIER = 4;

export interface UmbAuthSession {
	/** When the access token expires (issuedAt + expiresIn). Used to decide when to refresh. */
	accessTokenExpiresAt: number;
	/** When the full session expires (issuedAt + expiresIn * MULTIPLIER). Used by the worker for timeout UI. */
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

	// Cross-tab coordination
	#channel: BroadcastChannel;

	// Popup management
	#authWindowProxy?: WindowProxy | null;
	#popupCleanup?: () => void;

	/**
	 * @deprecated Observe isAuthorized instead. Scheduled for removal in Umbraco 19.
	 */
	readonly #authorizationSignal = new Subject<void>();

	// Track clients that have been configured to prevent duplicate interceptor binding
	#configuredClients = new WeakSet();

	// Endpoint URLs
	#linkEndpoint;
	#linkKeyEndpoint;
	#unlinkEndpoint;
	#postLogoutRedirectUri;

	/**
	 * Observable that emits true when the auth context is initialized.
	 * @remark It will only emit once and then complete itself.
	 */
	readonly isInitialized = this.#isInitialized.asObservable();

	/**
	 * Observable that emits true if the user is authorized, otherwise false.
	 * @remark It will only emit when the authorization state changes.
	 */
	readonly isAuthorized = this.#isAuthorized.asObservable().pipe(distinctUntilChanged());

	/**
	 * Observable that acts as a signal and emits when the user has timed out, i.e. the token has expired.
	 * This can be used to show a timeout message to the user.
	 * @remark It will emit once per second, so it can be used to trigger UI updates or other actions when the user has timed out.
	 */
	readonly timeoutSignal = this.#isTimeout.asObservable().pipe(
		// Audit the timeout signal to ensure that it waits for 1s before allowing another emission, which prevents rapid firing of the signal.
		// This is useful to prevent the UI from being flooded with timeout events.
		auditTime(1000),
	);

	/**
	 * Observable that acts as a signal for when the authorization state changes.
	 * @deprecated Observe isAuthorized instead. Scheduled for removal in Umbraco 19.
	 * @remark It will emit once per second, so it can be used to trigger UI updates or other actions when the authorization state changes.
	 * @returns An observable that emits when the authorization state changes.
	 */
	get authorizationSignal(): Observable<void> {
		new UmbDeprecation({
			deprecated: 'get authorizationSignal',
			solution:
				'Observe isAuthorized instead. This provides more useful information (authorized or not) and is more efficient to consume. Scheduled for removal in Umbraco 19.',
			removeInVersion: '19.0.0',
		}).warn();
		return this.#authorizationSignal.asObservable().pipe(
			// Throttle the signal to ensure that it emits once, then waits for 1s before allowing another emission.
			throttleTime(1000),
		);
	}

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

		this.#client = new UmbAuthClient(endpoints, redirectUri);

		// Set up cross-tab coordination via BroadcastChannel
		this.#channel = new BroadcastChannel('umb:auth');
		this.#channel.onmessage = (evt: MessageEvent) => {
			switch (evt.data?.type) {
				case 'authorized': {
					// Set session locally — do NOT call #updateSession which would re-broadcast
					const accessTokenExpiresAt = evt.data.issuedAt + evt.data.expiresIn;
					const expiresAt = evt.data.issuedAt + evt.data.expiresIn * TOKEN_EXPIRY_MULTIPLIER;
					this.#session.setValue({ accessTokenExpiresAt, expiresAt });
					this.#isAuthorized.setValue(true);
					this.#authorizationSignal.next();
					break;
				}
				case 'sessionUpdate':
					this.#session.setValue({
						accessTokenExpiresAt: evt.data.accessTokenExpiresAt,
						expiresAt: evt.data.expiresAt,
					});
					this.#isAuthorized.setValue(true);
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
					// Another tab is asking for the current session state (e.g. new tab opening)
					const session = this.#session.getValue();
					if (session) {
						this.#channel.postMessage({ type: 'sessionResponse', session });
					}
					break;
				}
			}
		};

		if (!isTestEnvironment()) {
			// Start the session timeout controller
			new UmbAuthSessionTimeoutController(this);
		}
	}

	override destroy(): void {
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
	) {
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
		// Resolves when authorized; also resolves (no-op) if the popup is closed/cancelled.
		return new Promise<void>((resolve) => {
			const cleanup = () => {
				clearInterval(closedPoll);
				this.#channel.removeEventListener('message', handler);
				window.removeEventListener('message', pkceHandler);
				this.#popupCleanup = undefined;
			};
			this.#popupCleanup = cleanup;

			const handler = (evt: MessageEvent) => {
				if (evt.data?.type === 'authorized') {
					cleanup();
					this.#client.clearPkceState();
					this.#authWindowProxy?.close();
					resolve();
				}
			};
			this.#channel.addEventListener('message', handler);

			// Poll for popup closed (user cancelled or closed the window)
			const closedPoll = setInterval(() => {
				if (this.#authWindowProxy?.closed) {
					cleanup();
					this.#client.clearPkceState();
					resolve();
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

		// Try to get PKCE state — first from the parent window (popup flow), then from sessionStorage (redirect flow)
		let codeVerifier: string | undefined;

		if (window.opener) {
			// Popup flow: request code_verifier from parent via postMessage
			codeVerifier = await this.#requestCodeVerifierFromOpener(state);
		}

		if (!codeVerifier) {
			// Redirect flow: read from sessionStorage
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
		this.#channel.postMessage({
			type: 'authorized',
			expiresIn: response.expiresIn,
			issuedAt: response.issuedAt,
		});

		// Fire the deprecated signal for external consumers
		this.#authorizationSignal.next();

		return response;
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
		// If we already have a session, no need to re-initialize
		if (this.#session.getValue()) {
			return;
		}

		// Ask existing tabs for their session state (avoids a /token call for new tabs)
		const peerSession = await this.#requestSessionFromPeers();
		if (peerSession) {
			this.#session.setValue(peerSession);
			this.#isAuthorized.setValue(true);
			return;
		}

		// No peer responded — try a server refresh.
		// Uses the Web Lock so concurrent calls (from API requests via getLatestToken)
		// are deduplicated — only one actual /token call is made.
		await this.makeRefreshTokenRequest();
	}

	/**
	 * Gets the latest token from the Management API.
	 * With cookie auth, this returns '[redacted]' — the real token is in the httpOnly cookie.
	 * If the session has expired, it will attempt a refresh first.
	 *
	 * @example <caption>Using the latest token</caption>
	 * ```js
	 *   const token = await authContext.getLatestToken();
	 *   const result = await fetch('https://my-api.com', { headers: { Authorization: `Bearer ${token}` } });
	 * ```
	 * @deprecated Use {@link configureClient} for `@hey-api/openapi-ts` clients or {@link getOpenApiConfiguration} for manual fetch calls. With cookie-based auth this always returns `'[redacted]'`. Scheduled for removal in Umbraco 19.
	 * @see {@link configureClient} for automatic token handling with `@hey-api/openapi-ts` clients.
	 * @see {@link getOpenApiConfiguration} for manual fetch calls with cookie-based auth.
	 * @memberof UmbAuthContext
	 * @returns The latest token from the Management API
	 */
	async getLatestToken(): Promise<string> {
		new UmbDeprecation({
			deprecated: 'getLatestToken',
			solution:
				'Use configureClient for @hey-api/openapi-ts clients or getOpenApiConfiguration for manual fetch calls. With cookie-based auth this always returns "[redacted]".',
			removeInVersion: '19.0.0',
		}).warn();
		return '[redacted]';
	}

	/**
	 * Validates the token against the server and returns true if the token is valid.
	 * Uses Web Locks to prevent concurrent refresh requests across tabs.
	 * @memberof UmbAuthContext
	 * @returns True if the token is valid, otherwise false
	 */
	async validateToken(): Promise<boolean> {
		return this.#isBypassed || this.makeRefreshTokenRequest();
	}

	/**
	 * Attempts to refresh the token using Web Locks to prevent concurrent refresh requests.
	 * @returns True if the refresh was successful, otherwise false.
	 */
	async makeRefreshTokenRequest(): Promise<boolean> {
		// Fallback for environments without Web Locks (some enterprise/kiosk browsers)
		if (!navigator.locks) {
			console.warn('[UmbAuth] navigator.locks is not available — token refresh coordination disabled.');
			const response = await this.#client.refreshToken();
			if (response) {
				this.#updateSession(response.expiresIn, response.issuedAt);
				return true;
			}
			return false;
		}

		// Capture the session before entering the lock queue. Inside the lock we check
		// if the session object was replaced — that means another tab broadcast a
		// sessionUpdate while we were waiting, so we can skip our own /token call.
		// We compare object references (not expiresAt) because keepUserLoggedIn triggers
		// a proactive refresh *before* the access token expires; an expiresAt-based check
		// would incorrectly skip the refresh when the session is still technically valid.
		const sessionBefore = this.#session.getValue();

		return navigator.locks.request('umb:token-refresh', async () => {
			if (this.#session.getValue() !== sessionBefore && this.isSessionValid()) return true;

			const response = await this.#client.refreshToken();
			if (response) {
				this.#updateSession(response.expiresIn, response.issuedAt);
				return true;
			}
			return false;
		});
	}

	/**
	 * Checks if the current session is still valid.
	 * @returns True if the session has not expired.
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
	 * @returns The server url to the Management API
	 */
	getServerUrl() {
		return this.#serverUrl;
	}

	/**
	 * Get the default OpenAPI configuration, which is set up to communicate with the Management API.
	 * @remark This is useful if you want to communicate with your own resources generated by the [@hey-api/openapi-ts](https://github.com/hey-api/openapi-ts) library.
	 * @memberof UmbAuthContext
	 * @example <caption>Using the default OpenAPI configuration</caption>
	 * ```js
	 * const defaultOpenApi = authContext.getOpenApiConfiguration();
	 * client.setConfig({
	 *   base: defaultOpenApi.base,
	 *   auth: defaultOpenApi.token,
	 * });
	 * ```
	 * @returns {UmbOpenApiConfiguration} The default OpenAPI configuration
	 */
	getOpenApiConfiguration(): UmbOpenApiConfiguration {
		return {
			base: this.#serverUrl,
			credentials: 'include',
			token: () => Promise.resolve('[redacted]'),
		};
	}

	/**
	 * Configures a `@hey-api/openapi-ts` client for authenticated API calls.
	 * Sets baseUrl, credentials, auth header, and binds the default response
	 * interceptors (401 retry, error handling, notifications).
	 * @example
	 * ```js
	 * const authContext = await this.getContext(UMB_AUTH_CONTEXT);
	 * authContext.configureClient(myClient);
	 * // Now myClient automatically includes auth headers and interceptors
	 * ```
	 * @param client A `@hey-api/openapi-ts` client instance.
	 */
	configureClient(client: typeof umbHttpClient) {
		if (this.#configuredClients.has(client)) return;
		this.#configuredClients.add(client);

		client.setConfig({
			baseUrl: this.#serverUrl,
			credentials: 'include',
			auth: () => '[redacted]',
		});

		// Controller self-registers on the host element via UmbControllerBase constructor,
		// so the anonymous reference is intentional — lifecycle is managed by the host.
		// Note: _host must be a proper UmbControllerHost (element host) for correct cleanup.
		new UmbApiInterceptorController(this._host).bindDefaultInterceptors(client);
	}

	/**
	 * Sets the auth context as initialized, which means that the auth context is ready to be used.
	 * @remark This is used to let the app context know that the core module is ready, which means that the core auth providers are available.
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
	 * @returns The redirect url, which is the backoffice path.
	 */
	getRedirectUrl() {
		return `${window.location.origin}${this.#backofficePath}${this.#backofficePath.endsWith('/') ? '' : '/'}oauth_complete`;
	}

	/**
	 * Gets the post logout redirect url.
	 * @returns The post logout redirect url, which is the backoffice path with the logout path appended.
	 */
	getPostLogoutRedirectUrl() {
		return `${window.location.origin}${this.#backofficePath}${this.#backofficePath.endsWith('/') ? '' : '/'}logout`;
	}

	/**
	 * Links the current user to the specified provider by redirecting to the link endpoint.
	 * @param provider The provider to link to.
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
	 * @param loginProvider
	 * @param providerKey
	 */
	async unlinkLogin(loginProvider: string, providerKey: string): Promise<boolean> {
		const request = new Request(this.#unlinkEndpoint, {
			method: 'POST',
			credentials: 'include',
			headers: { 'Content-Type': 'application/json', Authorization: 'Bearer [redacted]' },
			body: JSON.stringify({ loginProvider, providerKey }),
		});

		const result = await fetch(request);

		if (!result.ok) {
			const error = await result.json();
			throw error;
		}

		await this.signOut();

		return true;
	}

	/**
	 * Sets the in-memory session state without broadcasting.
	 * Use when the caller handles broadcasting separately (e.g. completeAuthorizationRequest).
	 */
	#setSessionLocally(expiresIn: number, issuedAt: number) {
		const accessTokenExpiresAt = issuedAt + expiresIn;
		// The access_token lives for 1/4 of the refresh_token lifetime.
		// Multiply to get the full session expiry.
		const expiresAt = issuedAt + expiresIn * TOKEN_EXPIRY_MULTIPLIER;
		this.#session.setValue({ accessTokenExpiresAt, expiresAt });
		this.#isAuthorized.setValue(true);
	}

	/**
	 * Updates the in-memory session state and broadcasts to other tabs.
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

	/**
	 * Asks other tabs for their current session state via BroadcastChannel.
	 * Returns the first response within 300ms, or undefined if no peer responds.
	 * The 300ms window is empirical — long enough for a loaded peer tab to respond
	 * via the event loop, short enough to not noticeably delay login.
	 */
	#requestSessionFromPeers(): Promise<UmbAuthSession | undefined> {
		return new Promise((resolve) => {
			const timeout = setTimeout(() => {
				this.#channel.removeEventListener('message', handler);
				resolve(undefined);
			}, 300);

			const handler = (evt: MessageEvent) => {
				if (evt.data?.type === 'sessionResponse' && evt.data.session) {
					clearTimeout(timeout);
					this.#channel.removeEventListener('message', handler);
					resolve(evt.data.session);
				}
			};

			this.#channel.addEventListener('message', handler);
			this.#channel.postMessage({ type: 'sessionRequest' });
		});
	}

	/**
	 * Requests the code_verifier from the parent window via postMessage (popup flow).
	 */
	#requestCodeVerifierFromOpener(state: string | null): Promise<string | undefined> {
		return new Promise((resolve) => {
			if (!window.opener) {
				resolve(undefined);
				return;
			}

			const timeout = setTimeout(() => {
				window.removeEventListener('message', handler);
				resolve(undefined);
			}, 5000);

			const handler = (evt: MessageEvent) => {
				if (evt.origin !== window.location.origin) return;
				if (evt.data?.type === 'pkceResponse' && evt.data?.state === state) {
					clearTimeout(timeout);
					window.removeEventListener('message', handler);
					resolve(evt.data.codeVerifier);
				}
			};

			window.addEventListener('message', handler);

			// Ask the parent for the code_verifier
			window.opener.postMessage({ type: 'pkceRequest', state }, window.location.origin);
		});
	}

	async #makeLinkTokenRequest(provider: string) {
		const request = await fetch(`${this.#linkKeyEndpoint}?provider=${provider}`, {
			credentials: 'include',
			headers: {
				Authorization: 'Bearer [redacted]',
				'Content-Type': 'application/json',
			},
		});

		if (!request.ok) {
			throw new Error('Failed to link login');
		}

		return request.json();
	}
}
