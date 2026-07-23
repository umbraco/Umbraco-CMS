import type { UmbAuthContext } from '../auth.context.js';
import { UMB_MODAL_AUTH_TIMEOUT } from '../modals/umb-auth-timeout-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Safety margin (in seconds) subtracted from the computed session expiry so the timeout modal
 * treats the session as expired slightly early. The client stamps the session timing when the
 * configuration response arrives, a touch after the server issued the cookie, so the estimate is
 * always mildly optimistic.
 */
const EXPIRY_SAFETY_MARGIN_IN_SECONDS = 5;

/** Maximum delay (in ms) that setTimeout supports (2^31 - 1). Larger delays fire immediately in browsers. */
const MAX_TIMEOUT_MS = 2_147_483_647;

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
					// A (re)established session — dismiss any open timeout modal (e.g. another tab
					// logged back in and broadcast the fresh session).
					this.#closeTimeoutModal();

					// keepUserLoggedIn: the server keeps the session alive, so we never warn or time
					// out client-side (never show the timeout overlay). Otherwise, schedule the warning
					// against the cookie's fixed expiry.
					if (!host.keepUserLoggedIn) {
						this.#scheduleCheck(session.expiresAt);
					}
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
	 * @param {number} expiresAt The unix timestamp (in seconds) when the session expires.
	 */
	#scheduleCheck(expiresAt: number) {
		this.#scheduledExpiresAt = expiresAt;

		const now = Math.floor(Date.now() / 1000);
		const secondsUntilExpiry = expiresAt - now;

		if (secondsUntilExpiry <= 0) {
			// Already expired
			this.#onSessionExpiring(expiresAt);
			return;
		}

		// Adaptive buffer: If less than one minutes, set it to 15s, otherwise 25% of session, clamped to [5s, 60s]
		const buffer = secondsUntilExpiry <= 60 ? 15 : Math.max(5, Math.min(60, Math.floor(secondsUntilExpiry * 0.25)));
		const secondsUntilWarning = secondsUntilExpiry - buffer;

		if (secondsUntilWarning <= 0) {
			// Already in the buffer zone
			this.#onSessionExpiring(expiresAt);
			return;
		}

		let delayMs = secondsUntilWarning * 1000;
		if (delayMs > MAX_TIMEOUT_MS) {
			// Not expected in practice: both session.expiresAt and session.accessTokenExpiresAt derive
			// from the server-issued token lifetime that Global:TimeOut governs, and server-side
			// validation bounds Global:TimeOut below setTimeout's ceiling.
			// Clamp and warn so a regression producing an out-of-range session lifetime surfaces here,
			// rather than setTimeout silently firing the check immediately.
			console.warn(
				`[Auth] Session warning delay ${delayMs}ms exceeds the maximum supported setTimeout delay; clamping to ${MAX_TIMEOUT_MS}ms.`,
			);
			delayMs = MAX_TIMEOUT_MS;
		}

		this.#timeoutId = setTimeout(() => this.#onSessionExpiring(expiresAt), delayMs);
	}

	/**
	 * Called when the session enters the warning zone or has expired. Shows the save-your-work
	 * timeout modal, or times out immediately (which redirects to the login screen) if already expired.
	 * @param {number} originalExpiresAt The session expiry (unix timestamp in seconds) this check was scheduled for.
	 */
	async #onSessionExpiring(originalExpiresAt: number) {
		// Guard: if the session was refreshed since we scheduled this check, skip.
		// We compare expiresAt rather than using isSessionValid() because this fires
		// during the buffer zone (before full expiry), when the session is still "valid".
		if (this.#scheduledExpiresAt !== originalExpiresAt) return;

		// Recompute the remaining time from the wall clock — the scheduled timer may have
		// fired late (system sleep, background-tab timer throttling), in which case the
		// session can already be expired even though it was valid when the timer was set.
		const secondsRemaining = originalExpiresAt - Math.floor(Date.now() / 1000) - EXPIRY_SAFETY_MARGIN_IN_SECONDS;

		if (secondsRemaining <= 0) {
			this.#host.timeOut();
			return;
		}

		await this.#openTimeoutModal(secondsRemaining);
	}

	async #closeTimeoutModal() {
		const contextToken = (await import('@umbraco-cms/backoffice/modal')).UMB_MODAL_MANAGER_CONTEXT;
		const modalManager = await this.getContext(contextToken);
		modalManager?.close('auth-timeout');
	}

	async #openTimeoutModal(remainingTimeInSeconds: number): Promise<void> {
		const contextToken = (await import('@umbraco-cms/backoffice/modal')).UMB_MODAL_MANAGER_CONTEXT;

		try {
			const modalManager = await this.getContext(contextToken);
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
						this.#tryKeepAlive();
					},
					onExpired: () => {
						this.#host.timeOut();
					},
				},
			});
			await modal?.onSubmit();
		} catch {
			// Modal was force-closed or errored near the end of the session — try to keep it alive
			// gracefully; #tryKeepAlive times out (→ redirect to login) if renewal fails.
			this.#tryKeepAlive();
		}
	}

	/**
	 * Renews the session via the keep-alive endpoint. Times the user out (which redirects to the
	 * login screen, preserving the current path via ReturnUrl) if renewal fails, so a dead session
	 * doesn't linger.
	 */
	async #tryKeepAlive(): Promise<void> {
		const success = await this.#host.keepAlive();
		if (!success) {
			this.#host.timeOut();
		}
	}
}
