import type { UmbAuthContext } from '../auth.context.js';
import type { UmbAuthFlow } from '../auth-flow.js';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbAuthSessionTimeoutController extends UmbControllerBase {
	#tokenCheckWorker?: SharedWorker;

	constructor(host: UmbAuthContext, authFlow: UmbAuthFlow) {
		super(host, 'UmbAuthSessionTimeoutController');

		this.#tokenCheckWorker = new SharedWorker(new URL('../workers/token-check.worker.js', import.meta.url), {
			name: 'TokenCheckWorker',
			type: 'module',
		});

		// Ensure the worker is ready to receive messages
		this.#tokenCheckWorker.port.start();

		// Listen for messages from the token check worker
		this.#tokenCheckWorker.port.onmessage = async (event) => {
			if (event.data?.command === 'logout') {
				// If the worker signals a logout, we clear the token storage and set the user as unauthorized
				host.timeOut();
			} else if (event.data?.command === 'refreshToken') {
				const secondsUntilLogout = event.data.secondsUntilLogout;
				console.log(
					'[Auth Context] Token check worker requested a refresh token. Refreshing in',
					secondsUntilLogout,
					'seconds.',
				);
				try {
					await umbConfirmModal(this, {
						headline: 'Session Expiring',
						cancelLabel: 'Logout',
						confirmLabel: 'Stay Logged In',
						content: `Your session is about to expire in ${secondsUntilLogout} seconds. Do you want to stay logged in?`,
					});
					host.validateToken().catch((error) => {
						console.error('[Auth Context] Error validating token:', error);
						// If the token validation fails, we clear the token storage and set the user as unauthorized
						host.timeOut();
					});
				} catch {
					host.timeOut();
				}
			}
		};

		// Initialize the token check worker with the current token response
		this.observe(
			authFlow.token$,
			(tokenResponse) => {
				// Inform the token check worker about the new token response
				console.log('[Auth Context] Informing token check worker about new token response.', this.#tokenCheckWorker);
				// Post the new
				this.#tokenCheckWorker?.port.postMessage({
					command: 'init',
					tokenResponse,
				});
			},
			'_authFlowAuthorizationSignal',
		);

		// Listen for the timeout signal to stop the token check worker
		// TODO: Close the timeout modal if it is open
		this.observe(host.timeoutSignal, () => {
			// Stop the token check worker when the user has timed out
			this.#tokenCheckWorker?.port.postMessage({
				command: 'init',
			});
		});
	}

	override destroy(): void {
		super.destroy();
		this.#tokenCheckWorker?.port.close();
		this.#tokenCheckWorker = undefined;
	}
}
