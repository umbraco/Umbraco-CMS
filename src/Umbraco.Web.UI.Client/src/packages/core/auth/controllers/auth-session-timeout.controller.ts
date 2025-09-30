import type { UmbAuthFlow } from '../auth-flow.js';
import type { UmbAuthContext } from '../auth.context.js';
import { UMB_MODAL_AUTH_TIMEOUT } from '../modals/umb-auth-timeout-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbAuthSessionTimeoutController extends UmbControllerBase {
	#tokenCheckWorker?: SharedWorker;
	#host: UmbAuthContext;
	#keepUserLoggedIn = false;
	#hasCheckedKeepUserLoggedIn = false;

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

		this.observe(
			host.isAuthorized,
			(isAuthorized) => {
				if (isAuthorized) {
					this.#observeKeepUserLoggedIn();
				}
			},
			'_authFlowIsAuthorizedSignal',
		);
	}

	override destroy(): void {
		super.destroy();
		this.#tokenCheckWorker?.port.close();
		this.#tokenCheckWorker = undefined;
	}

	/**
	 * Observe the user's preference for staying logged in
	 * and update the internal state accordingly.
	 * This method fetches the current user configuration from the server to find the value.
	 * // TODO: We cannot observe the config store directly here yet, as it would create a circular dependency, so maybe we need to move the config option somewhere else?
	 */
	async #observeKeepUserLoggedIn() {
		if (this.#hasCheckedKeepUserLoggedIn) return;
		this.#hasCheckedKeepUserLoggedIn = true;
		// eslint-disable-next-line local-rules/no-direct-api-import
		const { data } = await UserService.getUserCurrentConfiguration();
		this.#keepUserLoggedIn = data?.keepUserLoggedIn ?? false;
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
