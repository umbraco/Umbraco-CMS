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

const ports: MessagePort[] = [];

// eslint-disable-next-line @typescript-eslint/no-explicit-any
let interval: any;

console.log('[Token Check Worker] Token check worker initialized.');

/**
 * Define the globalThis object to handle the onconnect event as SharedWorkerGlobalScope.
 * This must be defined manually because TypeScript does not recognize the SharedWorkerGlobalScope type on all environments.
 */
const _self = globalThis as typeof globalThis & {
	onconnect: (event: MessageEvent) => void;
};

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
			console.log('[Token Check Worker] Token should be refreshed, but it is not expired yet.');
			// Let all connected clients know that the token should be refreshed
			for (const port of ports) {
				if (port) {
					port.postMessage({
						command: 'refreshToken',
						secondsUntilLogout: result.numberOfSecondsUntilExpiration,
					});
				}
			}
		}
	}, VALIDATION_INTERVAL);
}
