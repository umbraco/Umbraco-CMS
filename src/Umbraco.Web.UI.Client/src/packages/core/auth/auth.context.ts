import { UmbAuthFlow } from './auth-flow.js';
import { UMB_AUTH_CONTEXT } from './auth.context.token.js';
import type { UmbOpenApiConfiguration } from './models/openApiConfiguration.js';
import type { ManifestAuthProvider } from './auth-provider.extension.js';
import { UMB_STORAGE_TOKEN_RESPONSE_NAME } from './constants.js';
import { OpenAPI } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { ReplaySubject, Subject, firstValueFrom, switchMap } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbAuthContext extends UmbContextBase<UmbAuthContext> {
	#isAuthorized = new UmbBooleanState<boolean>(false);
	// Timeout is different from `isAuthorized` because it can occur repeatedly
	#isTimeout = new Subject<void>();
	#isInitialized = new ReplaySubject<void>(1);
	#isBypassed;
	#serverUrl;
	#backofficePath;
	#authFlow;

	#authWindowProxy?: WindowProxy | null;
	#previousAuthUrl?: string;

	/**
	 * Observable that emits true when the auth context is initialized.
	 * @remark It will only emit once and then complete itself.
	 */
	readonly isInitialized = this.#isInitialized.asObservable();

	/**
	 * Observable that emits true if the user is authorized, otherwise false.
	 * @remark It will only emit when the authorization state changes.
	 */
	readonly isAuthorized = this.#isAuthorized.asObservable();

	/**
	 * Observable that acts as a signal and emits when the user has timed out, i.e. the token has expired.
	 * This can be used to show a timeout message to the user.
	 * @remark It can emit multiple times if more than one request is made after the token has expired.
	 */
	readonly timeoutSignal = this.#isTimeout.asObservable();

	/**
	 * Observable that acts as a signal for when the authorization state changes.
	 */
	get authorizationSignal() {
		return this.#authFlow.authorizationSignal;
	}

	constructor(host: UmbControllerHost, serverUrl: string, backofficePath: string, isBypassed: boolean) {
		super(host, UMB_AUTH_CONTEXT);
		this.#isBypassed = isBypassed;
		this.#serverUrl = serverUrl;
		this.#backofficePath = backofficePath;

		this.#authFlow = new UmbAuthFlow(
			serverUrl,
			this.getRedirectUrl(),
			this.getPostLogoutRedirectUrl(),
			this.#isTimeout,
		);

		// Observe the authorization signal and close the auth window
		this.observe(
			this.authorizationSignal,
			() => {
				// Update the authorization state
				this.getIsAuthorized();
			},
			'_authFlowAuthorizationSignal',
		);

		// Observe changes to local storage and update the authorization state
		// This establishes the tab-to-tab communication
		window.addEventListener('storage', this.#onStorageEvent.bind(this));
	}

	override destroy(): void {
		super.destroy();
		window.removeEventListener('storage', this.#onStorageEvent.bind(this));
	}

	async #onStorageEvent(evt: StorageEvent) {
		if (evt.key === UMB_STORAGE_TOKEN_RESPONSE_NAME) {
			// Close any open auth windows
			this.#authWindowProxy?.close();
			// Refresh the local storage state into memory
			await this.setInitialState();
			// Let any auth listeners (such as the auth modal) know that the auth state has changed
			this.authorizationSignal.next();
		}
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
		const redirectUrl = await this.#authFlow.makeAuthorizationRequest(identityProvider, usernameHint);
		if (redirect) {
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

		return firstValueFrom(this.authorizationSignal);
	}

	/**
	 * Completes the login flow.
	 */
	completeAuthorizationRequest() {
		return this.#authFlow.completeAuthorizationIfPossible();
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
			const isAuthorized = this.#authFlow.isAuthorized();
			this.#isAuthorized.setValue(isAuthorized);
			return isAuthorized;
		}
	}

	/**
	 * Sets the initial state of the auth flow.
	 * @returns {Promise<void>}
	 */
	setInitialState(): Promise<void> {
		return this.#authFlow.setInitialState();
	}

	/**
	 * Gets the latest token from the Management API.
	 * If the token is expired, it will be refreshed.
	 *
	 * NB! The user may experience being redirected to the login screen if the token is expired.
	 * @example <caption>Using the latest token</caption>
	 * ```js
	 *   const token = await authContext.getLatestToken();
	 *   const result = await fetch('https://my-api.com', { headers: { Authorization: `Bearer ${token}` } });
	 * ```
	 * @memberof UmbAuthContext
	 * @returns The latest token from the Management API
	 */
	getLatestToken(): Promise<string> {
		return this.#authFlow.performWithFreshTokens();
	}

	/**
	 * Validates the token against the server and returns true if the token is valid.
	 * @memberof UmbAuthContext
	 * @returns True if the token is valid, otherwise false
	 */
	async validateToken(): Promise<boolean> {
		return this.#isBypassed || this.#authFlow.makeRefreshTokenRequest();
	}

	/**
	 * Clears the token storage.
	 * @memberof UmbAuthContext
	 */
	clearTokenStorage() {
		return this.#authFlow.clearTokenStorage();
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
	 * Signs the user out by removing any tokens from the browser.
	 * @memberof UmbAuthContext
	 */
	signOut(): Promise<void> {
		return this.#authFlow.signOut();
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
	 *   credentials: defaultOpenApi.credentials,
	 *   auth: defaultOpenApi.token,
	 * });
	 * ```
	 * @returns {UmbOpenApiConfiguration} The default OpenAPI configuration
	 */
	getOpenApiConfiguration(): UmbOpenApiConfiguration {
		return {
			base: OpenAPI.BASE,
			version: OpenAPI.VERSION,
			withCredentials: OpenAPI.WITH_CREDENTIALS,
			credentials: OpenAPI.CREDENTIALS,
			token: () => this.getLatestToken(),
			encodePath: OpenAPI.ENCODE_PATH,
		};
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
	 * @param provider
	 * @see UmbAuthFlow#linkLogin
	 */
	linkLogin(provider: string) {
		return this.#authFlow.linkLogin(provider);
	}

	/**
	 * @param providerName
	 * @param providerKey
	 * @see UmbAuthFlow#unlinkLogin
	 */
	unlinkLogin(providerName: string, providerKey: string) {
		return this.#authFlow.unlinkLogin(providerName, providerKey);
	}
}
