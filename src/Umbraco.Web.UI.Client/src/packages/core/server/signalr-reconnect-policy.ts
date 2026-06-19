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
	nextRetryDelayInMilliseconds(retryContext: RetryContext): number {
		return RECONNECT_DELAYS_MS[retryContext.previousRetryCount] ?? MAX_RECONNECT_DELAY_MS;
	}
}
