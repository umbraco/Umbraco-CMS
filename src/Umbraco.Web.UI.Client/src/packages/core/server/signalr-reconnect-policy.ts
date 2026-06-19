import type { IRetryPolicy, RetryContext } from '@umbraco-cms/backoffice/external/signalr';

const RECONNECT_DELAYS_MS = [0, 2000, 5000, 10000];
const MAX_RECONNECT_DELAY_MS = 30000;

/**
 * A SignalR retry policy that reconnects indefinitely with a capped backoff.
 * The default `withAutomaticReconnect()` policy gives up after ~60 seconds, which leaves an idle
 * backoffice (preview, server events) permanently disconnected after a transient drop — common when
 * SignalR has fallen back to Server-Sent Events and a keepalive is delayed by response buffering.
 */
export class UmbSignalRReconnectPolicy implements IRetryPolicy {
	/**
	 * Gets the delay before the next reconnect attempt, following the capped backoff schedule.
	 * @param {RetryContext} retryContext - The context for the current retry, including the previous retry count.
	 * @returns {number} The delay in milliseconds before the next reconnect attempt (never null, so retries continue indefinitely).
	 * @memberof UmbSignalRReconnectPolicy
	 */
	nextRetryDelayInMilliseconds(retryContext: RetryContext): number {
		return RECONNECT_DELAYS_MS[retryContext.previousRetryCount] ?? MAX_RECONNECT_DELAY_MS;
	}
}
