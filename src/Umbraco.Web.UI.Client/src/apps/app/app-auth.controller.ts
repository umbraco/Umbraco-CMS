import { UMB_AUTH_CONTEXT, UMB_MODAL_APP_AUTH, UMB_STORAGE_REDIRECT_URL } from '@umbraco-cms/backoffice/auth';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbAppAuthController extends UmbControllerBase {
	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#serverExtensionRegistrator = new UmbServerExtensionRegistrator(this, umbExtensionsRegistry);
	#init;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.#authContext = context;
		});

		this.#init = Promise.all([this.#serverExtensionRegistrator.registerPublicExtensions()]);
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
	async makeAuthorizationRequest(): Promise<boolean> {
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		await this.#init;

		// Figure out which providers are available
		const availableProviders = await firstValueFrom(umbExtensionsRegistry.byType('authProvider'));
		if (availableProviders.length === 0) {
			// No providers available, initiate the authorization request to the default provider
			this.#authContext.makeAuthorizationRequest();
		} else {
			// Check if any provider is redirecting directly to the provider, and if so, redirect to that provider
			const redirectProvider = availableProviders.find((provider) => provider.meta?.autoRedirect);
			if (redirectProvider) {
				this.#authContext.makeAuthorizationRequest(redirectProvider.forProviderName);
				return true;
			}

			// Show the provider selection screen
			console.log('show modal for', availableProviders);
			const modalContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
			await modalContext
				.open(this._host, UMB_MODAL_APP_AUTH)
				.onSubmit()
				.catch(() => undefined);
		}

		// Reinitialize the auth flow (load the state from local storage)
		this.#authContext.setInitialState();

		// The authorization flow is finished, so let the caller know if the user is authorized
		return this.#authContext.getIsAuthorized();
	}
}
