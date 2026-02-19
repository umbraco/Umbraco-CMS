import { UmbAuthClient } from './umb-auth-client.js';
import type { UmbAuthClientEndpoints, UmbTokenEndpointResponse } from './umb-auth-client.js';
import { UMB_AUTH_CONTEXT } from './auth.context.token.js';
import { UmbAuthSessionTimeoutController } from './controllers/auth-session-timeout.controller.js';
import type { UmbOpenApiConfiguration } from './models/openApiConfiguration.js';
import type { ManifestAuthProvider } from './auth-provider.extension.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
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
import { umbHttpClient } from '@umbraco-cms/backoffice/http-client';
import { isTestEnvironment } from '@umbraco-cms/backoffice/utils';

/**
 * The multiplier for the token expiry time.
 * In Umbraco, access_tokens live for a quarter of the time of the refresh_token.
 * We multiply by this to get the full session lifetime.
 */
const TOKEN_EXPIRY_MULTIPLIER = 4;

interface UmbAuthSession {
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
	#previousAuthUrl?: string;

	/**
	 * @deprecated Observe isAuthorized instead. Scheduled for removal in Umbraco 19.
	 */
	readonly #authorizationSignal = new Subject<void>();

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
		return this.#authorizationSignal.asObservable().pipe(
			// Throttle the signal to ensure that it emits once, then waits for 1s before allowing another emission.
			throttleTime(1000),
		);
	}

	constructor(host: UmbControllerHost, serverUrl: string, backofficePath: string, isBypassed: boolean) {
		super(host, UMB_AUTH_CONTEXT);
		this.#isBypassed = isBypassed;
		this.#serverUrl = serverUrl;
		this.#backofficePath = backofficePath;

		const redirectUri = this.getRedirectUrl();
		this.#postLogoutRedirectUri = this.getPostLogoutRedirectUrl();

		const endpoints: UmbAuthClientEndpoints = {
			authorizationEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/authorize`,
			tokenEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/token`,
			revocationEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/revoke`,
			endSessionEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/signout`,
			linkEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/link-login`,
			linkKeyEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/link-login-key`,
			unlinkEndpoint: `${serverUrl}/umbraco/management/api/v1/security/back-office/unlink-login`,
		};

		this.#linkEndpoint = endpoints.linkEndpoint;
		this.#linkKeyEndpoint = endpoints.linkKeyEndpoint;
		this.#unlinkEndpoint = endpoints.unlinkEndpoint;

		this.#client = new UmbAuthClient(endpoints, redirectUri);

		// Configure the shared http client for authenticated API calls
		this.#configureDefaultClient();

		// Set up cross-tab coordination via BroadcastChannel
		this.#channel = new BroadcastChannel('umb:auth');
		this.#channel.onmessage = (evt: MessageEvent) => {
			switch (evt.data?.type) {
				case 'authorized':
					this.#updateSession(evt.data.expiresIn, evt.data.issuedAt);
					this.#isAuthorized.setValue(true);
					this.#authorizationSignal.next();
					break;
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

		if (!this.#authWindowProxy || this.#authWindowProxy.closed) {
			this.#authWindowProxy = window.open(redirectUrl, popupTarget, popupFeatures);
		} else if (this.#previousAuthUrl !== redirectUrl) {
			this.#authWindowProxy = window.open(redirectUrl, popupTarget);
			this.#authWindowProxy?.focus();
		}

		this.#previousAuthUrl = redirectUrl;

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

		// Wait for the popup to complete via BroadcastChannel
		return new Promise<void>((resolve) => {
			const handler = (evt: MessageEvent) => {
				if (evt.data?.type === 'authorized') {
					this.#channel.removeEventListener('message', handler);
					window.removeEventListener('message', pkceHandler);
					this.#client.clearPkceState();
					// Close the popup window
					this.#authWindowProxy?.close();
					resolve();
				}
			};
			this.#channel.addEventListener('message', handler);
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
					}
				} catch {
					// Ignore parse errors
				}
				sessionStorage.removeItem('umb:pkce');
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

		this.#updateSession(response.expiresIn, response.issuedAt);
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
	 * With cookie auth, this is lightweight — no localStorage reads.
	 * If a session is already set (e.g. from BroadcastChannel), this is a no-op.
	 * @returns {Promise<void>}
	 */
	async setInitialState(): Promise<void> {
		// If we already have a session, no need to re-initialize
		if (this.#session.getValue()) {
			return;
		}

		// Try a token refresh to establish session timing.
		// The httpOnly cookie carries the real refresh token.
		const response = await this.#client.refreshToken();
		if (response) {
			this.#updateSession(response.expiresIn, response.issuedAt);
		}
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
	 * @memberof UmbAuthContext
	 * @returns The latest token from the Management API
	 */
	async getLatestToken(): Promise<string> {
		const session = this.#session.getValue();

		// If the access token is still valid, return immediately
		if (session && session.accessTokenExpiresAt > Math.floor(Date.now() / 1000)) {
			return '[redacted]';
		}

		// Access token expired — try to refresh
		const success = await this.makeRefreshTokenRequest();
		if (!success) {
			this.clearTokenStorage();
		}

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
		// Capture session state before acquiring the lock so we can detect
		// if another tab refreshed while we waited.
		const sessionBeforeLock = this.#session.getValue();

		return navigator.locks.request('umb:token-refresh', async () => {
			// If the session changed while we waited for the lock, another tab already refreshed
			const currentSession = this.#session.getValue();
			if (currentSession && currentSession !== sessionBeforeLock) {
				return true;
			}

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
		this.#channel.postMessage({ type: 'sessionCleared' });
	}

	/**
	 * Handles the case where the user has timed out, i.e. the token has expired.
	 * This will clear the token storage and set the user as unauthorized.
	 * @memberof UmbAuthContext
	 */
	timeOut() {
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

		// Clear internal state
		this.clearTokenStorage();

		// Redirect to end session endpoint
		const postLogoutRedirectUri = new URL(this.#postLogoutRedirectUri, window.origin);
		const endSessionEndpoint = `${this.#serverUrl}/umbraco/management/api/v1/security/back-office/signout`;
		const postLogoutLocation = new URL(endSessionEndpoint, this.getRedirectUrl());
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
	 * 	const serverUrl = authContext.getServerUrl();
	 * 	const token = await authContext.getLatestToken();
	 * 	const result = await fetch(`${serverUrl}/umbraco/management/api/v1/my-resource`, { headers: { Authorization: `Bearer ${token}` } });
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
			token: () => this.getLatestToken(),
		};
	}

	/**
	 * Configures a @hey-api/openapi-ts client for authenticated API calls.
	 * Use this when you have a custom API client generated from your own OpenAPI spec.
	 * @example
	 * ```js
	 * const authContext = await this.getContext(UMB_AUTH_CONTEXT);
	 * authContext.configureClient(myClient);
	 * // Now myClient automatically includes auth headers
	 * ```
	 * @param client A client with a setConfig method (e.g. from @hey-api/openapi-ts).
	 */
	configureClient(client: { setConfig: (config: Record<string, unknown>) => void }) {
		client.setConfig({
			baseUrl: this.#serverUrl,
			credentials: 'include',
			auth: () => this.getLatestToken(),
		});
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
		const token = await this.getLatestToken();
		const request = new Request(this.#unlinkEndpoint, {
			method: 'POST',
			credentials: 'include',
			headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
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
	 * Configures the default umbHttpClient with auth settings.
	 */
	#configureDefaultClient() {
		umbHttpClient.setConfig({
			baseUrl: this.#serverUrl,
			credentials: 'include',
			auth: () => this.getLatestToken(),
		});
	}

	/**
	 * Updates the in-memory session state and broadcasts to other tabs.
	 */
	#updateSession(expiresIn: number, issuedAt: number) {
		const accessTokenExpiresAt = issuedAt + expiresIn;
		// The access_token lives for 1/4 of the refresh_token lifetime.
		// Multiply to get the full session expiry.
		const expiresAt = issuedAt + expiresIn * TOKEN_EXPIRY_MULTIPLIER;
		this.#session.setValue({ accessTokenExpiresAt, expiresAt });
		this.#isAuthorized.setValue(true);

		// Broadcast the session update to other tabs
		this.#channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt, expiresAt });
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
		const token = await this.getLatestToken();

		const request = await fetch(`${this.#linkKeyEndpoint}?provider=${provider}`, {
			credentials: 'include',
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
		});

		if (!request.ok) {
			throw new Error('Failed to link login');
		}

		return request.json();
	}
}
