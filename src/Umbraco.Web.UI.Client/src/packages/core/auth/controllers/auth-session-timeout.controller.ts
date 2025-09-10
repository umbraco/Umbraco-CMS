import type { UmbAuthFlow } from '../auth-flow.js';
import type { UmbAuthContext } from '../auth.context.js';
import { UMB_MODAL_AUTH_TIMEOUT } from '../modals/umb-auth-timeout-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbCurrentUserConfigRepository } from '@umbraco-cms/backoffice/current-user';

export class UmbAuthSessionTimeoutController extends UmbControllerBase {
	#tokenCheckWorker?: SharedWorker;
	#host: UmbAuthContext;
	#currentUserConfig = new UmbCurrentUserConfigRepository(this);
	#keepUserLoggedIn = false;

	constructor(host: UmbAuthContext, authFlow: UmbAuthFlow) {
		super(host, 'UmbAuthSessionTimeoutController');

		this.#host = host;

		this.#tokenCheckWorker = new SharedWorker(new URL('../workers/token-check.worker.js', import.meta.url), {
			name: 'TokenCheckWorker',
			type: 'module',
		});

		// Ensure the worker is ready to receive messages
		this.#tokenCheckWorker.port.start();

		// Listen for messages from the token check worker
		this.#tokenCheckWorker.port.onmessage = async (event) => {
			// If the user has chosen to stay logged in, we ignore the logout command and instead request a new token
			if (this.#keepUserLoggedIn) {
				console.log(
					'[Auth Context] User chose to stay logged in, attempting to validate token instead of logging out.',
				);
				await this.#tryValidateToken();
				return;
			}

			if (event.data?.command === 'logout') {
				// If the worker signals a logout, we clear the token storage and set the user as unauthorized
				host.timeOut();
			} else if (event.data?.command === 'refreshToken') {
				// If the worker signals a token refresh, we let the user decide whether to continue or logout
				this.#openTimeoutModal(event.data.secondsUntilLogout);
			}
		};

		// Initialize the token check worker with the current token response
		this.observe(
			authFlow.token$,
			(tokenResponse) => {
				// Inform the token check worker about the new token response
				console.log('[Auth Context] Informing token check worker about new token response.');
				// Post the new
				this.#tokenCheckWorker?.port.postMessage({
					command: 'init',
					tokenResponse,
				});
			},
			'_authFlowAuthorizationSignal',
		);

		// Listen for the timeout signal to stop the token check worker
		this.observe(
			host.timeoutSignal,
			async () => {
				// Stop the token check worker when the user has timed out
				this.#tokenCheckWorker?.port.postMessage({
					command: 'init',
				});

				// Close the modal if it is open
				await this.#closeTimeoutModal();
			},
			'_authFlowTimeoutSignal',
		);

		this.#observeCurrentUser();
	}

	override destroy(): void {
		super.destroy();
		this.#tokenCheckWorker?.port.close();
		this.#tokenCheckWorker = undefined;
	}

	async #observeCurrentUser() {
		await this.#currentUserConfig.initialized;

		// Listen for current user config to see if we should stay logged in
		this.observe(
			this.#currentUserConfig.part('keepUserLoggedIn'),
			(keepUserLoggedIn) => {
				this.#keepUserLoggedIn = keepUserLoggedIn;
			},
			'_authFlowKeepUserLoggedIn',
		);
	}

	async #closeTimeoutModal() {
		const contextToken = (await import('@umbraco-cms/backoffice/modal')).UMB_MODAL_MANAGER_CONTEXT;
		const modalManager = await this.getContext(contextToken);
		modalManager?.close('auth-timeout');
	}

	async #openTimeoutModal(remainingTimeInSeconds: number) {
		const contextToken = (await import('@umbraco-cms/backoffice/modal')).UMB_MODAL_MANAGER_CONTEXT;
		const modalManager = await this.getContext(contextToken);
		modalManager
			?.open(this, UMB_MODAL_AUTH_TIMEOUT, {
				modal: {
					key: 'auth-timeout',
				},
				data: {
					remainingTimeInSeconds,
					onLogout: () => {
						this.#host.signOut();
					},
					onContinue: () => {
						// If the user chooses to stay logged in, we validate the token
						this.#tryValidateToken();
					},
				},
			})
			.onSubmit()
			.catch(() => {
				// If the modal is forced closed or an error occurs, we handle it gracefully
				this.#tryValidateToken();
			});
	}

	async #tryValidateToken() {
		try {
			await this.#host.validateToken();
		} catch (error) {
			console.error('[Auth Context] Error validating token:', error);
			// If the token validation fails, we clear the token storage and set the user as unauthorized
			this.#host.timeOut();
		}
	}
}
