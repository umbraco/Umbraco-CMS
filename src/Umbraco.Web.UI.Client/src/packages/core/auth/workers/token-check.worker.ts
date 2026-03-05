import type { TokenResponse } from '@umbraco-cms/backoffice/external/openid';

/**
 * Applies a buffer time before the actual expiration of the token.
 * This is to ensure that we have time to refresh the token before it expires.
 */
const BUFFER_BEFORE_EXPIRATION = 60; // 1 minute in seconds

/**
 * The interval at which the token will be validated.
 */
const VALIDATION_INTERVAL = 30000; // 30 seconds in milliseconds

/**
 * The multiplier for the token expiry time.
 * In Umbraco, access_tokens live for a quarter of the time of the refresh_token.
 */
const TOKEN_EXPIRY_MULTIPLIER = 4;

/**
 * A common interface for broadcasting messages to connected clients,
 * abstracting the difference between SharedWorker ports and regular Worker scope.
 */
interface MessageTarget {
	postMessage(data: unknown): void;
}

const messageTargets: MessageTarget[] = [];

// eslint-disable-next-line @typescript-eslint/no-explicit-any
let interval: any;

console.log('[Token Check Worker] Token check worker initialized.');

/**
 * Handles incoming commands from the main thread.
 */
function handleCommand(e: MessageEvent) {
	if (e.data?.command === 'init') {
		console.log('[Token Check Worker] Initializing with token response:', e.data.tokenResponse);
		if (e.data.tokenResponse) {
			init(e.data.tokenResponse);
		} else if (interval) {
			console.warn('[Token Check Worker] No token response provided, stopping the interval.');
			clearInterval(interval);
			interval = undefined;
		}
	}
}

/**
 * Support both SharedWorker and regular (dedicated) Worker modes.
 * SharedWorker is not available on all platforms (e.g. Chrome on Android),
 * so the worker must gracefully handle being loaded as a regular Worker.
 */
const isSharedWorkerScope = 'SharedWorkerGlobalScope' in globalThis;

if (isSharedWorkerScope) {
	// SharedWorker mode: handle connections from multiple tabs
	const _self = globalThis as typeof globalThis & {
		onconnect: (event: MessageEvent) => void;
	};

	_self.onconnect = (event: MessageEvent) => {
		console.log('[Token Check Worker] Connected to main thread (SharedWorker).');
		const port = event.ports[0];

		messageTargets.push(port);

		// Listen for messages from any port connected to this worker
		port.onmessage = (e: MessageEvent) => handleCommand(e);
	};
} else {
	// Regular (dedicated) Worker mode: single connection
	console.log('[Token Check Worker] Running as dedicated Worker.');
	messageTargets.push(globalThis as unknown as MessageTarget);
	globalThis.onmessage = (e: MessageEvent) => handleCommand(e);
}

/**
 * Checks if the provided token is expired.
 * If the token is not provided, it is considered expired.
 * @param {TokenResponse} token - The token response to check.
 * @returns {object} An object containing:
 * - tokenIsExpired: A boolean indicating if the token is expired.
 * - numberOfSecondsUntilExpiration: The number of seconds until the token expires, or 0 if it is expired.
 */
function isTokenExpired(token: TokenResponse) {
	const currentTime = Math.floor(Date.now() / 1000);
	const tokenExpiresAt = token.issuedAt + (token.expiresIn ?? 1) * TOKEN_EXPIRY_MULTIPLIER;
	const tokenExpiresInSeconds = tokenExpiresAt - currentTime;

	console.log('[Token Check Worker] Token expires in', tokenExpiresInSeconds, 'seconds');

	let tokenExpiresInSecondsWithBuffer = tokenExpiresInSeconds - BUFFER_BEFORE_EXPIRATION;
	if (tokenExpiresInSecondsWithBuffer < 0) {
		tokenExpiresInSecondsWithBuffer = 0;
	}

	const tokenIsExpired = tokenExpiresInSecondsWithBuffer === 0;

	return {
		tokenIsExpired,
		numberOfSecondsUntilExpiration: tokenExpiresInSecondsWithBuffer,
	};
}

/**
 * This worker checks the token expiration at regular intervals.
 * If the token is expired or missing, it will trigger a logout.
 * @param {TokenResponse} tokenResponse - The token response to check.
 */
function init(tokenResponse: TokenResponse) {
	console.log('[Token Check Worker] Initializing token check worker...');

	if (interval) {
		clearInterval(interval);
	}

	interval = setInterval(() => {
		const result = isTokenExpired(tokenResponse);

		if (result.tokenIsExpired) {
			console.log('[Token Check Worker] Token is expired or missing, triggering logout.');
			// Trigger logout logic here, e.g., send a message to all connected clients
			for (const target of messageTargets) {
				if (target) {
					target.postMessage({ command: 'logout' });
				}
			}
			clearInterval(interval);
			interval = undefined;
			console.log('[Token Check Worker] Waiting for token refresh...');
		} else if (result.numberOfSecondsUntilExpiration <= BUFFER_BEFORE_EXPIRATION) {
			console.log('[Token Check Worker] Token should be refreshed, but it is not expired yet.');
			// Let all connected clients know that the token should be refreshed
			for (const target of messageTargets) {
				if (target) {
					target.postMessage({
						command: 'refreshToken',
						secondsUntilLogout: result.numberOfSecondsUntilExpiration,
					});
				}
			}
		}
	}, VALIDATION_INTERVAL);
}
