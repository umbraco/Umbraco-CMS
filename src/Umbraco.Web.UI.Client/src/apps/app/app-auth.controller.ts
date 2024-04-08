import {
	UMB_AUTH_CONTEXT,
	UMB_MODAL_APP_AUTH,
	UMB_STORAGE_REDIRECT_URL,
	type UmbUserLoginState,
} from '@umbraco-cms/backoffice/auth';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { filter, firstValueFrom, skip } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbAppAuthController extends UmbControllerBase {
	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#firstTimeLoggingIn = true;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.#authContext = context;

			// Observe the user's authorization state and start the authorization flow if the user is not authorized
			this.observe(
				context.isAuthorized.pipe(
					// Skip the first since it is always false
					skip(1),
					// Only continue if the value is false
					filter((x) => !x),
				),
				() => {
					this.#firstTimeLoggingIn = false;
					this.makeAuthorizationRequest('timedOut');
				},
				'_authState',
			);
		});
	}

	/**
	 * Checks if the user is authorized.
	 * If not, the authorization flow is started.
	 */
	async isAuthorized(): Promise<boolean> {
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		const isAuthorized = this.#authContext.getIsAuthorized();

		if (isAuthorized) {
			return true;
		}

		// Save location.href so we can redirect to it after login
		window.sessionStorage.setItem(UMB_STORAGE_REDIRECT_URL, location.href);

		// Make a request to the auth server to start the auth flow
		return this.makeAuthorizationRequest();
	}

	/**
	 * Starts the authorization flow.
	 * It will check which providers are available and either redirect directly to the provider or show a provider selection screen.
	 */
	async makeAuthorizationRequest(userLoginState: UmbUserLoginState = 'loggingIn'): Promise<boolean> {
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		// If the user is timed out, we can show the login modal directly
		if (userLoginState === 'timedOut') {
			const selected = await this.#showLoginModal(userLoginState);

			if (!selected) {
				return false;
			}

			return this.#updateState();
		}

		// Figure out which providers are available
		const availableProviders = await firstValueFrom(this.#authContext.getAuthProviders());

		if (availableProviders.length === 0) {
			throw new Error('[Fatal] No auth providers available');
		}

		if (availableProviders.length === 1) {
			// One provider available (most likely the Umbraco provider), so initiate the authorization request to the default provider
			this.#authContext.makeAuthorizationRequest();
		} else {
			// Check if any provider is redirecting directly to the provider
			const redirectProvider = availableProviders.find((provider) => provider.meta?.behavior?.autoRedirect);

			if (redirectProvider) {
				// Redirect directly to the provider
				this.#authContext.makeAuthorizationRequest(redirectProvider.forProviderName);
			} else {
				// Show the provider selection screen
				const selected = await this.#showLoginModal(userLoginState);

				if (!selected) {
					return false;
				}
			}
		}

		return this.#updateState();
	}

	async #showLoginModal(userLoginState: UmbUserLoginState): Promise<boolean> {
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		// Show the provider selection screen
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const selected = await modalManager
			.open(this._host, UMB_MODAL_APP_AUTH, {
				data: {
					userLoginState,
				},
				modal: {
					backdropBackground: this.#firstTimeLoggingIn
						? 'var(--umb-auth-backdrop, rgba(0, 0, 0, 0.75))'
						: 'var(--umb-auth-backdrop-timedout, #000)',
				},
			})
			.onSubmit()
			.catch(() => undefined);

		if (!selected?.providerName) {
			return false;
		}

		this.#authContext.makeAuthorizationRequest(selected.providerName, selected.loginHint);

		return true;
	}

	#updateState() {
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		// Reinitialize the auth flow (load the state from local storage)
		this.#authContext.setInitialState();

		// The authorization flow is finished, so let the caller know if the user is authorized
		return this.#authContext.getIsAuthorized();
	}
}
