import { UMB_AUTH_CONTEXT } from './auth.context.token.js';
import { UmbAuthSessionTimeoutController } from './controllers/auth-session-timeout.controller.js';
import type { UmbOpenApiConfiguration } from './models/openApiConfiguration.js';
import type { ManifestAuthProvider } from './auth-provider.extension.js';
import { isReturnableRoute } from './returnable-route.function.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbApiInterceptorController, UMB_AUTH_SIGNALER_CONTEXT } from '@umbraco-cms/backoffice/resources';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { ReplaySubject, Subject, distinctUntilChanged, auditTime, map } from '@umbraco-cms/backoffice/external/rxjs';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApiClient, umbHttpClient } from '@umbraco-cms/backoffice/http-client';
import { isTestEnvironment, UmbDeprecation } from '@umbraco-cms/backoffice/utils';

export interface UmbAuthSession {
	/**
	 * @deprecated Cookie auth has a single, server-owned expiry, so this is now identical to
	 * {@link expiresAt}. Use `expiresAt`. Scheduled for removal in Umbraco 21.
	 */
	accessTokenExpiresAt?: number;
	/**
	 * When the session (auth cookie) expires. Used for the timeout UI.
	 * Undefined when the server reported no expiry, which means "unknown", not "never": the
	 * countdown is not scheduled at all rather than run against a guess.
	 */
	expiresAt?: number;
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
	 * Observable that emits once, without a value, when the auth context is initialized.
	 * For consumers: the boot sequence already awaits app entry points before the router evaluates
	 * its guards, so by the time any extension code runs this has long since completed.
	 * @internal
	 * @deprecated Internal boot signal, never intended for public use. Scheduled for removal in Umbraco 19.
	 * @remarks It will only emit once and then complete itself.
	 * @returns {Observable<void>} An observable that emits once when the auth context is initialized.
	 */
	get isInitialized(): Observable<void> {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.isInitialized',
			solution:
				'Remove the dependency on this signal. It is an internal boot detail — the app awaits app entry points before routing, so extension code always runs after initialization. Scheduled for removal in Umbraco 19.',
			removeInVersion: '19.0.0',
		}).warn();
		return this.#isInitializedObservable;
	}

	/** Internal, non-warning view of {@link UmbAuthContext#isInitialized} for use inside this class. */
	readonly #isInitializedObservable = this.#isInitialized.asObservable();

	/**
	 * Observable that emits true if the user is authorized, otherwise false.
	 * @remarks It will only emit when the authorization state changes.
	 */
	readonly isAuthorized = this.#session.asObservable().pipe(
		map((session) => this.#isBypassed || !!session),
		distinctUntilChanged(),
	);

	/**
	 * Observable that acts as a signal and emits when the user has timed out, i.e. the token has expired.
	 * This can be used to show a timeout message to the user.
	 * @remarks It will emit once per second, so it can be used to trigger UI updates or other actions when the user has timed out.
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
	 * @param {string} identityProvider The provider to use for login. Default is 'Umbraco'.
	 * @param {boolean} redirect If true, the user will be redirected to the login page.
	 * @param {string} usernameHint The username hint to use for login.
	 * @param _usernameHint
	 * @param {ManifestAuthProvider} manifest The manifest for the registered provider.
	 */
	async makeAuthorizationRequest(
		identityProvider = 'Umbraco',
		redirect?: boolean,
		_usernameHint?: string,
		manifest?: ManifestAuthProvider,
	): Promise<void> {
		// Preserve where the user was, but only when it is somewhere worth returning to.
		const deepLink = isReturnableRoute(window.location.pathname, this.#backofficePath)
			? window.location.pathname + window.location.search
			: undefined;

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

		// Read the renewed expiry back rather than deriving it: the server is the only authority on
		// when the cookie now expires, and the keep-alive response does not carry it.
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
			// No expiry reported means the server did not tell us when the session ends, not that it
			// never does. Guessing a lifetime schedules the warning at the wrong time in whichever
			// direction the guess is wrong, so carry the session without one instead.
			const expiresIn = data.timeoutUtc
				? Math.max(0, Math.floor(new Date(data.timeoutUtc).getTime() / 1000) - issuedAt)
				: undefined;

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
	 * Checks if the current session is still valid.
	 * @deprecated Use {@link getIsAuthorized} or observe {@link session$} instead. Scheduled for removal in Umbraco 19.
	 * @returns {boolean} True if the session has not expired.
	 */
	isSessionValid(): boolean {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.isSessionValid()',
			solution:
				'Use getIsAuthorized() for a synchronous check, or observe session$ to react to session changes. Scheduled for removal in Umbraco 19.',
			removeInVersion: '19.0.0',
		}).warn();
		return this.#isSessionValid();
	}

	/**
	 * Internal, non-warning implementation of {@link isSessionValid}.
	 * @returns {boolean} True if the session has not expired.
	 */
	#isSessionValid(): boolean {
		const session = this.#session.getValue();
		if (!session) return false;
		// An unknown expiry cannot be checked, so do not treat it as expired.
		return session.expiresAt === undefined || session.expiresAt > Math.floor(Date.now() / 1000);
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
		// signedOut covers other tabs.
		this.#session.setValue(undefined);
		this.#channel.postMessage({ type: 'signedOut' });

		// The server sign-out endpoint clears the auth cookie, then redirects to the client logout landing
		// (derived server-side from BackOfficeHost).
		location.href = `${this.#serverUrl}/umbraco/management/api/v1/security/back-office/signout`;
	}

	/**
	 * Get the server url to the Management API.
	 * @deprecated Consume UMB_SERVER_CONTEXT and use its `getServerUrl()` — the canonical source for the server URL. Scheduled for removal in Umbraco 21.
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
	 * @deprecated Consume UMB_SERVER_CONTEXT and use its `getServerUrl()` — the canonical source for the server URL. Scheduled for removal in Umbraco 19.
	 * @returns {string} The server url to the Management API
	 */
	getServerUrl() {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.getServerUrl()',
			solution:
				'Consume UMB_SERVER_CONTEXT from @umbraco-cms/backoffice/server and use its getServerUrl(), which is the canonical source. Scheduled for removal in Umbraco 19.',
			removeInVersion: '19.0.0',
		}).warn();
		return this.#serverUrl;
	}

	/**
	 * Get the default OpenAPI configuration, which is set up to communicate with the Management API.
	 * @remarks This is useful if you want to communicate with your own resources generated by the [@hey-api/openapi-ts](https://github.com/hey-api/openapi-ts) library.
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
			// Kept, unlike the removed token accessors: returning undefined is harmless, because the
			// hey-api SDK omits the Authorization header entirely rather than sending a bad one.
			token: async () => {
				new UmbDeprecation({
					deprecated: 'UmbOpenApiConfiguration.token',
					removeInVersion: '21.0.0',
					solution: 'The auth cookie is sent automatically with credentials: "include"; remove the token() call.',
				}).warn();
				return undefined;
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
	 * or one regenerated by an extension package against its own OpenAPI document.
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
	 * No code outside Umbraco core should ever call this — doing so opens the provider-discovery gate early.
	 * @internal
	 * @deprecated Internal boot hook, never intended for public use. Scheduled for removal in Umbraco 19.
	 * @remarks The constructor already does this, so calling it again is a no-op on an
	 * already-completed subject. It emits once, without a value.
	 */
	setInitialized() {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.setInitialized()',
			solution:
				'Do not call this. It is an internal boot hook owned by the core entry point. Scheduled for removal in Umbraco 19.',
			removeInVersion: '19.0.0',
		}).warn();
		this.#setInitialized();
	}

	/** Internal, non-warning implementation of {@link setInitialized}. */
	#setInitialized() {
		this.#isInitialized.next();
		this.#isInitialized.complete();
	}

	/**
	 * Gets all registered auth providers.
	 * @deprecated Query the extension registry directly: `umbExtensionsRegistry.byType('authProvider')`. Scheduled for removal in Umbraco 19.
	 * @remarks The initialization gate this used to add is redundant — the app awaits app entry points
	 * before the router evaluates its guards, so the provider list has already settled by then.
	 * @param {UmbBackofficeExtensionRegistry} extensionsRegistry The extension registry to query.
	 * @returns {Observable<Array<ManifestAuthProvider>>} An observable of the registered auth providers.
	 */
	getAuthProviders(extensionsRegistry: UmbBackofficeExtensionRegistry) {
		new UmbDeprecation({
			deprecated: 'UmbAuthContext.getAuthProviders()',
			solution:
				"Query the extension registry directly with byType('authProvider'). The boot sequence already awaits app entry points before routing, so the provider list has settled. Scheduled for removal in Umbraco 19.",
			removeInVersion: '19.0.0',
		}).warn();
		return this.#getAuthProviders(extensionsRegistry);
	}

	/**
	 * Internal, non-warning implementation of {@link getAuthProviders}.
	 * No initialization gate: the constructor completes #isInitialized, so piping through it only
	 * added a micro-task delay. Ordering is guaranteed by the boot sequence awaiting app entry
	 * points before the router evaluates its guards.
	 * @param {UmbBackofficeExtensionRegistry} extensionsRegistry The extension registry to query.
	 * @returns {Observable<Array<ManifestAuthProvider>>} An observable of the registered auth providers.
	 */
	#getAuthProviders(extensionsRegistry: UmbBackofficeExtensionRegistry) {
		return extensionsRegistry.byType<'authProvider', ManifestAuthProvider>('authProvider');
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
	 * @param {string} loginProvider The login provider to unlink from.
	 * @param {string} providerKey The provider's key for the current user.
	 * @returns {Promise<boolean>} True if the unlink succeeded.
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
	 * @param {number | undefined} expiresIn The number of seconds until the session expires, or
	 * undefined when the server reported no expiry.
	 * @param {number} issuedAt The timestamp when the session was issued.
	 */
	#setSessionLocally(expiresIn: number | undefined, issuedAt: number) {
		// Cookie auth: the session has a single, server-owned expiry (the auth cookie's), so both
		// timestamps are the same — the historical access-vs-refresh token split (and its ×4
		// multiplier) no longer applies. TODO (V21): drop the deprecated accessTokenExpiresAt.
		const expiresAt = expiresIn === undefined ? undefined : issuedAt + expiresIn;
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
