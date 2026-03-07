import type { UmbAuthContext } from '../auth.context.js';
import { UMB_MODAL_AUTH_TIMEOUT } from '../modals/umb-auth-timeout-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbAuthSessionTimeoutController extends UmbControllerBase {
	#host: UmbAuthContext;
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
					// Session refreshed — close any open timeout modal and schedule next check.
					// When keepUserLoggedIn is true, schedule based on the access token expiry
					// so we proactively refresh before API calls get 401s.
					// When false, schedule based on the full session expiry for the timeout modal.
					this.#closeTimeoutModal();
					const expiresAt = host.keepUserLoggedIn ? session.accessTokenExpiresAt : session.expiresAt;
					this.#scheduleCheck(expiresAt);
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
			this.#onSessionExpiring(0, expiresAt);
			return;
		}

		// Adaptive buffer: 25% of session, clamped to [5s, 60s]
		const buffer = Math.max(5, Math.min(60, Math.floor(secondsUntilExpiry * 0.25)));
		const secondsUntilWarning = secondsUntilExpiry - buffer;

		if (secondsUntilWarning <= 0) {
			// Already in the buffer zone
			this.#onSessionExpiring(secondsUntilExpiry, expiresAt);
		} else {
			this.#timeoutId = setTimeout(() => this.#onSessionExpiring(buffer, expiresAt), secondsUntilWarning * 1000);
		}
	}

	/**
	 * Called when the session is expiring or has expired.
	 * Decides whether to auto-refresh, show the timeout modal, or time out.
	 */
	async #onSessionExpiring(secondsRemaining: number, originalExpiresAt: number) {
		// Guard: if the session was refreshed since we scheduled this check, skip.
		// We compare expiresAt rather than using isSessionValid() because this fires
		// during the buffer zone (before full expiry), when the session is still "valid".
		if (this.#scheduledExpiresAt !== originalExpiresAt) return;

		if (this.#host.keepUserLoggedIn) {
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
		// Fallback for environments without Web Locks — every tab shows the modal
		if (!navigator.locks) {
			await this.#openTimeoutModal(secondsRemaining);
			return;
		}

		const acquired = await navigator.locks.request('umb:timeout-modal', { ifAvailable: true }, async (lock) => {
			if (!lock) return false;
			// We're the leader — show the modal. Lock is held until the modal closes.
			await this.#openTimeoutModal(secondsRemaining);
			return true;
		});

		if (!acquired) {
			// Another tab is showing the modal. Set a fallback for full expiry.
			// Cleared by #clearScheduledCheck when the leader broadcasts a sessionUpdate.
			this.#timeoutId = setTimeout(() => {
				if (!this.#host.isSessionValid()) {
					this.#host.timeOut();
				}
			}, secondsRemaining * 1000);
		}
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
			const modal = modalManager?.open(this, UMB_MODAL_AUTH_TIMEOUT, {
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
			});
			await modal?.onSubmit();
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
