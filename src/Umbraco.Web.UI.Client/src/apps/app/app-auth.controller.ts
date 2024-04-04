import { UMB_AUTH_CONTEXT, UMB_STORAGE_REDIRECT_URL } from '@umbraco-cms/backoffice/auth';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

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

		this.#authContext.makeAuthorizationRequest();

		// Reinitialize the auth flow (load the state from local storage)
		this.#authContext.setInitialState();

		return true;
	}
}
