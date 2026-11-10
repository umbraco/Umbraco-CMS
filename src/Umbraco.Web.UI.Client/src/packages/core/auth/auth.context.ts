import { UMB_AUTH_CONTEXT } from './auth.context.token.js';
import { UmbAuthSessionTimeoutController } from './controllers/auth-session-timeout.controller.js';
import type { UmbOpenApiConfiguration } from './models/openApiConfiguration.js';
import type { ManifestAuthProvider } from './auth-provider.extension.js';
import { isReturnableRoute } from './returnable-route.function.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbApiInterceptorController, UMB_AUTH_SIGNALER_CONTEXT } from '@umbraco-cms/backoffice/resources';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { Subject, distinctUntilChanged, auditTime, map } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApiClient, umbHttpClient } from '@umbraco-cms/backoffice/http-client';
import { isTestEnvironment, UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/** Delays (in ms) before each attempt to look up the user behind a new sign-in. */
const SIGNED_IN_USER_LOOKUP_DELAYS_MS = [0, 500, 1500];

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
	#isBypassed;
	#serverUrl;
	#backofficePath;

	// Session timing — in-memory only, no localStorage
	#session = new UmbObjectState<UmbAuthSession | undefined>(undefined);
	readonly session$ = this.#session.asObservable();

	// The window the current session's expiry describes, remembered so activity can slide the expiry
	// forward locally. Milliseconds, because activity is timestamped from the wall clock.
	#sessionWindowInMs?: number;

	// Cross-tab coordination
	#channel: BroadcastChannel;

	// The user the client state of this page belongs to, see bindUser().
	#boundUser?: string;
	// Set once the session has passed to another user. The page holds the previous user's state, so no
	// session is applied to it from then on.
	#hasUserChanged = false;
	// Applies the newest session that waits for the user behind it to be confirmed.
	#heldSession?: () => void;
	#confirmingUser?: Promise<boolean>;
	#userChanged = new Subject<void>();

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
	 * Emits when the session has passed to a different user than the one the page is bound to
	 * (see `bindUser()`), or to a user who could not be determined. The new session is not applied:
	 * the page holds client state that belongs to the previous user, so it has to start over rather than
	 * show that state to the user who is signed in now. From then on no session is applied to the page, so
	 * the host has to start it over, or the page stays unauthorized. The app shell handles this by reloading
	 * the backoffice. A sign-in while the page is still open emits again, so the host can try once more.
	 * A user who cannot be determined (every lookup failed) is treated as a different user on purpose, so the
	 * page also starts over, and discards unsaved work, when it is in fact the same user.
	 * @internal
	 */
	readonly userChanged = this.#userChanged.asObservable();

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
					void this.#applySession(true, () => this.#setSessionLocally(evt.data.expiresIn, evt.data.issuedAt));
					break;
				}
				case 'sessionUpdate': {
					// Another tab learned a fresher expiry, typically by sliding it on activity. The auth
					// cookie is shared between tabs, so the renewal applies here too. A session this tab no
					// longer holds is re-established, possibly by a sign-in it missed.
					void this.#applySession(!this.#session.getValue(), () =>
						this.#applySessionExpiry(evt.data.expiresAt, evt.data.sessionWindowInMs),
					);
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
			// Every request bearing the session renews it server-side, so follow that here rather than
			// counting down to the expiry the session started out with.
			this.observe(signaler?.activityDetectedAt, (activityDetectedAt) => {
				if (activityDetectedAt) {
					this.#slideSessionExpiry(activityDetectedAt);
				}
			});
			signaler?.setSessionCheck(() => this.keepAlive());
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

			// A session this page does not hold may come from a sign-in, possibly by another user
			const applied = this.#applySession(!this.#session.getValue(), () => this.#setSessionLocally(expiresIn, issuedAt));

			// Tell other tabs a session is (re)established, so they can update their local state too.
			this.#channel.postMessage({ type: 'authorized', expiresIn, issuedAt });
			return await applied;
		} catch {
			this.#session.setValue(undefined);
			return false;
		}
	}

	/**
	 * Binds the page to the user it has loaded client state for. A sign-in that happens after this, in this
	 * tab or another one, is held until the user behind it is known: the same user resumes the page as it
	 * is, so unsaved work survives; any other user is reported through {@link userChanged} instead.
	 * Binding a different user later is reported the same way. The app shell calls this with the user it has loaded.
	 * @internal
	 * @param {string} unique The unique of the user the page has loaded.
	 */
	bindUser(unique: string): void {
		if (!this.#boundUser) {
			this.#boundUser = unique;
			return;
		}
		if (unique !== this.#boundUser) {
			this.#onUserChanged();
		}
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
		if (expiresIn === undefined) {
			this.#applySessionExpiry(undefined);
			return;
		}

		// Every request bearing the session re-issues it for another full window, so the lifetime that
		// came with a just-learned expiry is that whole window.
		this.#applySessionExpiry(issuedAt + expiresIn, expiresIn * 1000);
	}

	/**
	 * Sets the in-memory session to a known expiry and remembers the window it describes.
	 * @param {number | undefined} expiresAt When the session expires (unix seconds), or undefined when
	 * the expiry is unknown.
	 * @param {number} [sessionWindowInMs] The window the expiry describes. Falls back to the time still
	 * to run, for an expiry that arrives without its window — the same value, less however long it took
	 * to get here.
	 */
	#applySessionExpiry(expiresAt: number | undefined, sessionWindowInMs?: number) {
		this.#sessionWindowInMs =
			expiresAt === undefined ? undefined : (sessionWindowInMs ?? expiresAt * 1000 - Date.now());

		// Cookie auth: the session has a single, server-owned expiry (the auth cookie's), so both
		// timestamps are the same — the historical access-vs-refresh token split (and its ×4
		// multiplier) no longer applies. TODO (V21): drop the deprecated accessTokenExpiresAt.
		this.#session.setValue({ accessTokenExpiresAt: expiresAt, expiresAt });
	}

	/**
	 * Applies a session established after the page loaded. A session that may come from a sign-in can belong
	 * to another user than the one the page is bound to, so it is held until that user is known. Sessions
	 * arriving while a sign-in is held wait behind it, so the newest one is applied once the user is confirmed.
	 * @param {boolean} fromSignIn Whether the session may come from a sign-in rather than a renewal of the session in hand.
	 * @param {() => void} apply Applies the session.
	 * @returns {Promise<boolean>} True once the session is applied, false when it belongs to another user.
	 */
	#applySession(fromSignIn: boolean, apply: () => void): Promise<boolean> {
		if (this.#hasUserChanged) {
			// The page is still open, so it has not started over yet: a further sign-in reports the change again
			if (fromSignIn) this.#userChanged.next();
			return Promise.resolve(false);
		}
		if (this.#isBypassed || !this.#boundUser || (!fromSignIn && !this.#confirmingUser)) {
			apply();
			return Promise.resolve(true);
		}

		this.#heldSession = apply;
		this.#confirmingUser ??= this.#confirmSignedInUser();
		return this.#confirmingUser;
	}

	async #confirmSignedInUser(): Promise<boolean> {
		const signedInUser = await this.#lookUpSignedInUser();
		const apply = this.#heldSession;
		this.#heldSession = undefined;
		this.#confirmingUser = undefined;

		if (apply && signedInUser === this.#boundUser) {
			apply();
			return true;
		}

		this.#onUserChanged();
		return false;
	}

	/**
	 * Looks up the user behind the session cookie, retrying failed attempts.
	 * Deliberately not through the configured API clients: their interceptors hold a request that is
	 * rejected for authorization until the user is signed in, which is what is being decided here.
	 * @returns {Promise<string | undefined>} The unique of the signed-in user, or undefined if it could not be determined.
	 */
	async #lookUpSignedInUser(): Promise<string | undefined> {
		for (const delay of SIGNED_IN_USER_LOOKUP_DELAYS_MS) {
			if (delay) await new Promise((resolve) => setTimeout(resolve, delay));
			const response = await this.#fetchWithCookie(`${this.#serverUrl}/umbraco/management/api/v1/user/current`, 'GET');
			if (!response?.ok) continue;
			const user = await response.json().catch(() => undefined);
			if (typeof user?.id === 'string') return user.id;
		}
		return undefined;
	}

	#onUserChanged() {
		if (this.#hasUserChanged) return;
		this.#hasUserChanged = true;
		this.#heldSession = undefined;
		this.#userChanged.next();
	}

	/**
	 * Slides the in-memory session expiry forward to mirror the renewal the server performs for any
	 * request bearing the session. Deliberately no round trip of its own: the renewal already happened
	 * on the request that produced this activity, so only the local bookkeeping is behind.
	 * @param {number} activityAt The timestamp (ms) of the response whose request renewed the session.
	 */
	#slideSessionExpiry(activityAt: number) {
		const session = this.#session.getValue();
		const sessionWindowInMs = this.#sessionWindowInMs;

		// Without a session, or with an expiry we never knew, there is nothing to slide — an unknown
		// expiry stays unknown rather than becoming a guess (see #establishSessionFromServer).
		if (!session?.expiresAt || sessionWindowInMs === undefined) {
			return;
		}

		// Rounded up, so that the expiry keeps up with activity instead of being rounded back to the
		// second it was already set to. The timeout controller's own safety margin covers the sub-second
		// optimism that adds.
		const expiresAt = Math.ceil((activityAt + sessionWindowInMs) / 1000);

		// The expiry only ever moves forward. This also settles what a burst of requests costs: they
		// resolve to the expiry that is already set, so nothing is published for them.
		if (expiresAt <= session.expiresAt) {
			return;
		}

		this.#applySessionExpiry(expiresAt, sessionWindowInMs);

		// Peer tabs share the cookie, so their expiry moved too — they just didn't make the request.
		this.#channel.postMessage({ type: 'sessionUpdate', expiresAt, sessionWindowInMs });
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
