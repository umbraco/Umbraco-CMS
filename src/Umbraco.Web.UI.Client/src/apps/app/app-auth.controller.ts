import { UMB_AUTH_CONTEXT, UMB_MODAL_APP_AUTH } from '@umbraco-cms/backoffice/auth';
import type { ManifestAuthProvider, UmbUserLoginState } from '@umbraco-cms/backoffice/auth';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { umbOpenModal, UmbPersistentModalDialogElement } from '@umbraco-cms/backoffice/modal';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import { retrieveStoredPath, setStoredPath } from '@umbraco-cms/backoffice/utils';

export class UmbAppAuthController extends UmbControllerBase {
	#retrievedModal: Promise<unknown>;
	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#timedOutRequest?: Promise<boolean>;
	#backofficePath?: string;
	#restarting = false;

	constructor(host: UmbControllerHost) {
		super(host);

		// Registered as the app starts, so it runs before the unsaved-changes guards that are registered later
		window.addEventListener('beforeunload', this.#onBeforeUnload, { capture: true });

		this.#retrievedModal = this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.#authContext = context;

			// Observe the user's authorization state and start the authorization flow if the user is not authorized
			this.observe(
				context?.timeoutSignal,
				() => {
					// One sign-in flow per session end: a timeout signalled while it is open joins it
					if (this.#timedOutRequest) return;
					console.log('[UmbAppAuthController] Authorization timed out, starting authorization flow');
					this.#timedOutRequest = this.makeAuthorizationRequest('timedOut').finally(() => {
						this.#timedOutRequest = undefined;
					});
				},
				'_authState',
			);

			// Observing an undefined source still invokes the callback, so only act on a context
			this.observe(
				context?.userChanged,
				() => {
					if (context) this.restartForNewUser();
				},
				'_userChanged',
			);
		}).asPromise({ preventTimeout: true });

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (currentUser) => {
			this.observe(
				currentUser?.unique,
				(unique) => {
					if (unique) this.#authContext?.bindUser(unique);
				},
				'_bindUser',
			);
		});

		this.consumeContext(UMB_SERVER_CONTEXT, (serverContext) => {
			this.#backofficePath = serverContext?.getBackofficePath();
		});
	}

	#onBeforeUnload = (event: Event) => {
		if (this.#restarting) event.stopImmediatePropagation();
	};

	/**
	 * Starts the backoffice over with a full page load, for the user who is signed in now. Only a new page
	 * guarantees that none of the previous user's client state, including state held by extensions, is
	 * shown to them. Unsaved changes on the page belong to the previous user, so no unsaved-changes
	 * prompt (`beforeunload`) can keep the page.
	 */
	restartForNewUser() {
		this.#restarting = true;
		// The path the previous user would have returned to after signing in
		retrieveStoredPath();
		location.href = this.#backofficePath ?? '/umbraco';
	}

	/**
	 * Checks if the user is authorized.
	 * If not, the authorization flow is started.
	 * Session verification is handled by setInitialState() before the router evaluates guards.
	 * @returns {Promise<boolean>} True if the user is authorized.
	 */
	async isAuthorized(): Promise<boolean> {
		await this.#retrievedModal.catch(() => undefined);
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		if (this.#authContext.getIsAuthorized()) {
			return true;
		}

		// Make a request to the auth server to start the auth flow
		return this.makeAuthorizationRequest();
	}

	/**
	 * Starts the authorization flow.
	 * It will check which providers are available and either redirect directly to the provider or show a provider selection screen.
	 * @param {UmbUserLoginState} userLoginState The reason the authorization flow is being started.
	 * @returns {Promise<boolean>} True if the user is authorized.
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

		// Figure out which providers are available. Queried straight from the registry: app.element.ts
		// awaits app entry points before routing, so any providers they register (or unregister) have
		// already settled by the time this runs.
		const availableProviders = await firstValueFrom(
			umbExtensionsRegistry.byType<'authProvider', ManifestAuthProvider>('authProvider'),
		);

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
		const selected = await umbOpenModal(this._host, UMB_MODAL_APP_AUTH, {
			data: {
				userLoginState,
			},
			modal: {
				type: 'custom',
				element: UmbPersistentModalDialogElement,
				backdropBackground: 'var(--umb-auth-backdrop, var(--uui-color-surface))',
			},
		}).catch(() => undefined);

		return selected?.success ?? false;
	}

	#updateState() {
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		// The authorization flow is finished, so let the caller know if the user is authorized
		return this.#authContext.getIsAuthorized();
	}

	override destroy() {
		window.removeEventListener('beforeunload', this.#onBeforeUnload, { capture: true });
		super.destroy();
	}
}
