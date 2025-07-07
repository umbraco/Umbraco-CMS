import type { TokenResponse } from '@umbraco-cms/backoffice/external/openid';

/**
 * Applies a buffer time before the actual expiration of the token.
 * This is to ensure that we have time to refresh the token before it expires.
 */
const BUFFER_BEFORE_EXPIRATION = 5; // seconds

/**
 * The interval at which the token will be validated.
 */
const VALIDATION_INTERVAL = 5000; // 1 minute

const ports: MessagePort[] = [];
let interval: NodeJS.Timeout | undefined;

console.log('[Token Check Worker] Token check worker initialized.');

const _self = globalThis as unknown as SharedWorkerGlobalScope & typeof globalThis;

_self.onconnect = (event: MessageEvent) => {
	console.log('[Token Check Worker] Connected to main thread.');
	const port = event.ports[0];

	ports.push(port);

	// Listen for messages from any port connected to this worker
	port.onmessage = (e: MessageEvent) => {
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
	};
};

/**
 * Checks if the provided token is expired.
 * If the token is not provided, it is considered expired.
 */
function isTokenExpired(token: TokenResponse) {
	const currentTime = Math.floor(Date.now() / 1000);
	const tokenExpiresAt = token.issuedAt + (token.expiresIn ?? 1) * 4; // Multiply by 4 because the token is refreshed 4 times before it expires
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
			// Trigger logout logic here, e.g., send a message to all connected ports
			for (const port of ports) {
				if (port) {
					port.postMessage({ command: 'logout' });
				}
			}
			clearInterval(interval);
			interval = undefined;
			console.log('[Token Check Worker] Waiting for token refresh...');
		} else if (result.numberOfSecondsUntilExpiration <= BUFFER_BEFORE_EXPIRATION) {
			console.log('[Token Check Worker] Token should be refreshed, but not expired yet.');
			// Let the main thread know that the token should be refreshed
			ports?.[0]?.postMessage({
				command: 'refreshToken',
				secondsUntilLogout: result.numberOfSecondsUntilExpiration,
			});
		}
	}, VALIDATION_INTERVAL);
}
