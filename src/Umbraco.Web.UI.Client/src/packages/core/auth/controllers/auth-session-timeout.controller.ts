import type { UmbAuthContext } from '../auth.context.js';
import { UMB_MODAL_AUTH_TIMEOUT } from '../modals/umb-auth-timeout-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbAuthSessionTimeoutController extends UmbControllerBase {
	#host: UmbAuthContext;
	#keepUserLoggedIn = false;
	#hasCheckedKeepUserLoggedIn = false;
	#timeoutId?: ReturnType<typeof setTimeout>;
	#scheduledExpiresAt?: number;

	constructor(host: UmbAuthContext) {
		super(host, 'UmbAuthSessionTimeoutController');

		this.#host = host;

		// When the session changes, reschedule the expiry check.
		// This fires when: initial login, token refresh (this or another tab), BroadcastChannel update.
		this.observe(
			host.session$,
			(session) => {
				this.#clearScheduledCheck();

				if (session) {
					// Session refreshed — close any open timeout modal and schedule next check
					this.#closeTimeoutModal();
					this.#scheduleCheck(session.expiresAt);
				}
			},
			'_sessionState',
		);

		// When the user times out, clean up
		this.observe(
			host.timeoutSignal,
			async () => {
				this.#clearScheduledCheck();
				await this.#closeTimeoutModal();
			},
			'_timeoutSignal',
		);

		// Check keepUserLoggedIn preference once authorized
		this.observe(
			host.isAuthorized,
			(isAuthorized) => {
				if (isAuthorized) {
					this.#observeKeepUserLoggedIn();
				}
			},
			'_isAuthorized',
		);
	}

	override destroy(): void {
		super.destroy();
		this.#clearScheduledCheck();
	}

	#clearScheduledCheck() {
		if (this.#timeoutId !== undefined) {
			clearTimeout(this.#timeoutId);
			this.#timeoutId = undefined;
		}
	}

	/**
	 * Schedules a check for when the session enters the warning zone (buffer before expiry).
	 * Uses adaptive buffer: 25% of session lifetime, clamped between 5s and 60s.
	 */
	#scheduleCheck(expiresAt: number) {
		this.#scheduledExpiresAt = expiresAt;

		const now = Math.floor(Date.now() / 1000);
		const secondsUntilExpiry = expiresAt - now;

		if (secondsUntilExpiry <= 0) {
			// Already expired
			this.#onSessionExpiring(0);
			return;
		}

		// Adaptive buffer: 25% of session, clamped to [5s, 60s]
		const buffer = Math.max(5, Math.min(60, Math.floor(secondsUntilExpiry * 0.25)));
		const secondsUntilWarning = secondsUntilExpiry - buffer;

		if (secondsUntilWarning <= 0) {
			// Already in the buffer zone
			this.#onSessionExpiring(secondsUntilExpiry);
		} else {
			this.#timeoutId = setTimeout(() => this.#onSessionExpiring(buffer), secondsUntilWarning * 1000);
		}
	}

	/**
	 * Called when the session is expiring or has expired.
	 * Decides whether to auto-refresh, show the timeout modal, or time out.
	 */
	async #onSessionExpiring(secondsRemaining: number) {
		// Guard: if session was refreshed since we scheduled this check, skip
		if (this.#host.isSessionValid()) return;

		if (this.#keepUserLoggedIn) {
			console.log('[Auth] Session expiring, auto-refreshing (keepUserLoggedIn=true)');
			const success = await this.#tryValidateToken();
			if (!success) {
				this.#host.timeOut();
			}
			return;
		}

		if (secondsRemaining <= 0) {
			console.log('[Auth] Session fully expired');
			this.#host.timeOut();
			return;
		}

		// Show timeout modal — only one tab via Web Lock leader election
		await this.#showTimeoutModalAsLeader(secondsRemaining);
	}

	/**
	 * Uses a Web Lock to ensure only one tab shows the timeout modal.
	 * Non-leader tabs set a fallback timeout for when the session fully expires.
	 */
	async #showTimeoutModalAsLeader(secondsRemaining: number) {
		const acquired = await navigator.locks.request(
			'umb:timeout-modal',
			{ ifAvailable: true },
			async (lock) => {
				if (!lock) return false;
				// We're the leader — show the modal. Lock is held until the modal closes.
				await this.#openTimeoutModal(secondsRemaining);
				return true;
			},
		);

		if (!acquired) {
			// Another tab is showing the modal. Set a fallback for full expiry.
			this.#timeoutId = setTimeout(() => {
				if (!this.#host.isSessionValid()) {
					this.#host.timeOut();
				}
			}, secondsRemaining * 1000);
		}
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

	async #openTimeoutModal(remainingTimeInSeconds: number): Promise<void> {
		const contextToken = (await import('@umbraco-cms/backoffice/modal')).UMB_MODAL_MANAGER_CONTEXT;
		const modalManager = await this.getContext(contextToken);

		try {
			await modalManager
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
							this.#tryValidateToken();
						},
					},
				})
				.onSubmit();
		} catch {
			// Modal was force-closed or an error occurred — try to refresh gracefully
			this.#tryValidateToken();
		}
	}

	async #tryValidateToken(): Promise<boolean> {
		try {
			return await this.#host.validateToken();
		} catch (error) {
			console.error('[Auth] Error validating token:', error);
			this.#host.timeOut();
			return false;
		}
	}
}
