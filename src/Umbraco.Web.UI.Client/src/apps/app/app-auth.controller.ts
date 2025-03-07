import { UMB_AUTH_CONTEXT, UMB_MODAL_APP_AUTH, type UmbUserLoginState } from '@umbraco-cms/backoffice/auth';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { setStoredPath } from '@umbraco-cms/backoffice/utils';

export class UmbAppAuthController extends UmbControllerBase {
	#retrievedModal: Promise<unknown>;
	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#isFirstCheck = true;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#retrievedModal = this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.#authContext = context;

			// Observe the user's authorization state and start the authorization flow if the user is not authorized
			this.observe(
				context.timeoutSignal,
				() => {
					this.makeAuthorizationRequest('timedOut');
				},
				'_authState',
			);
		}).asPromise();
	}

	/**
	 * Checks if the user is authorized.
	 * If not, the authorization flow is started.
	 */
	async isAuthorized(): Promise<boolean> {
		await this.#retrievedModal.catch(() => undefined);
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		const isAuthorized = this.#authContext.getIsAuthorized();

		if (isAuthorized) {
			// If this is the first time we are checking the authorization state (i.e. on first load), we need to make sure
			// that the token is still valid. If it is not, we need to start the authorization flow.
			// If the token is still valid, we can return true.
			if (this.#isFirstCheck) {
				this.#isFirstCheck = false;
				const isValid = await this.#authContext.validateToken();
				if (isValid) {
					return true;
				}
			} else {
				return true;
			}
		}

		// Make a request to the auth server to start the auth flow
		return this.makeAuthorizationRequest();
	}

	/**
	 * Starts the authorization flow.
	 * It will check which providers are available and either redirect directly to the provider or show a provider selection screen.
	 * @param userLoginState
	 */
	async makeAuthorizationRequest(userLoginState: UmbUserLoginState = 'loggingIn'): Promise<boolean> {
		await this.#retrievedModal.catch(() => undefined);
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		// Save the current state
		let currentUrl = window.location.href;
		const searchParams = new URLSearchParams(window.location.search);
		if (searchParams.has('returnPath')) {
			currentUrl = decodeURIComponent(searchParams.get('returnPath') || currentUrl);
		}
		setStoredPath(currentUrl);

		// Figure out which providers are available
		const availableProviders = await firstValueFrom(this.#authContext.getAuthProviders(umbExtensionsRegistry));

		if (availableProviders.length === 0) {
			throw new Error('[Fatal] No auth providers available');
		}

		// If we are logging in, we need to check if we can redirect directly to the provider
		if (userLoginState === 'loggingIn') {
			// One provider available (most likely the Umbraco provider), so initiate the authorization request to the default provider
			if (availableProviders.length === 1) {
				await this.#authContext.makeAuthorizationRequest(availableProviders[0].forProviderName, true);
				return this.#updateState();
			}

			// Check if any provider is redirecting directly to the provider
			const redirectProvider = availableProviders.find((provider) => provider.meta?.behavior?.autoRedirect);

			// Redirect directly to the provider
			if (redirectProvider) {
				await this.#authContext.makeAuthorizationRequest(redirectProvider.forProviderName, true);
				return this.#updateState();
			}
		}

		// Otherwise we can show the provider selection screen directly, because the user is either logged out, timed out, or has more than one provider available
		const selected = await this.#showLoginModal(userLoginState);

		if (!selected) {
			return false;
		}

		return this.#updateState();
	}

	async #showLoginModal(userLoginState: UmbUserLoginState): Promise<boolean> {
		await this.#retrievedModal.catch(() => undefined);
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		// Show the provider selection screen
		const authModalKey = 'umbAuthModal';
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) {
			throw new Error('[Fatal] Modal manager is not available');
		}

		const selected = await modalManager
			.open(this._host, UMB_MODAL_APP_AUTH, {
				data: {
					userLoginState,
				},
				modal: {
					key: authModalKey,
					backdropBackground: 'var(--umb-auth-backdrop, rgb(244, 244, 244))',
				},
			})
			.onSubmit()
			.catch(() => undefined);

		if (!selected?.success) {
			return false;
		}

		return true;
	}

	#updateState() {
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		// The authorization flow is finished, so let the caller know if the user is authorized
		return this.#authContext.getIsAuthorized();
	}
}
