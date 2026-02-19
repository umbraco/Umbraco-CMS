import type { UmbAuthContext } from '../auth.context.js';
import { UMB_MODAL_AUTH_TIMEOUT } from '../modals/umb-auth-timeout-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbAuthSessionTimeoutController extends UmbControllerBase {
	#tokenCheckWorker?: SharedWorker;
	#host: UmbAuthContext;
	#keepUserLoggedIn = false;
	#hasCheckedKeepUserLoggedIn = false;

	constructor(host: UmbAuthContext) {
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
			if (event.data?.command === 'sessionState') {
				// New tab: worker is sending us the current session state
				// This is handled via the auth context's setInitialState/BroadcastChannel
				return;
			}

			if (event.data?.command === 'logout') {
				if (this.#keepUserLoggedIn) {
					// Last resort: try to refresh before giving up
					console.log(
						'[Auth Context] Session fully expired, but user chose to stay logged in. Attempting last-resort refresh.',
					);
					const success = await this.#tryValidateToken();
					if (!success) {
						host.timeOut();
					}
					return;
				}
				// If the worker signals a logout, we clear the token storage and set the user as unauthorized
				host.timeOut();
			} else if (event.data?.command === 'refreshToken') {
				if (this.#keepUserLoggedIn) {
					console.log(
						'[Auth Context] User chose to stay logged in, attempting to validate token instead of showing timeout.',
					);
					// Check if session is already valid (another tab may have refreshed)
					if (host.isSessionValid()) {
						return;
					}
					await this.#tryValidateToken();
					return;
				}
				// If the worker signals a token refresh, we let the user decide whether to continue or logout
				this.#openTimeoutModal(event.data.secondsUntilLogout);
			}
		};

		// Initialize the token check worker with the current session state
		this.observe(
			host.session$,
			(session) => {
				if (session) {
					// Inform the token check worker about the new session expiry
					console.log('[Auth Context] Informing token check worker about new session state.');
					this.#tokenCheckWorker?.port.postMessage({
						command: 'init',
						expiresAt: session.expiresAt,
					});
				} else {
					// No session — stop the worker's interval
					this.#tokenCheckWorker?.port.postMessage({
						command: 'init',
					});
				}
			},
			'_authFlowSessionState',
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

	async #tryValidateToken(): Promise<boolean> {
		try {
			return await this.#host.validateToken();
		} catch (error) {
			console.error('[Auth Context] Error validating token:', error);
			// If the token validation fails, we clear the token storage and set the user as unauthorized
			this.#host.timeOut();
			return false;
		}
	}
}
