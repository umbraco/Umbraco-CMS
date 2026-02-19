/**
 * Applies a buffer time before the actual expiration of the token.
 * This is to ensure that we have time to refresh the token before it expires.
 */
const BUFFER_BEFORE_EXPIRATION = 60; // 1 minute in seconds

/**
 * The interval at which the token will be validated.
 */
const VALIDATION_INTERVAL = 30000; // 30 seconds in milliseconds

const ports: MessagePort[] = [];

// Current session state (shared across all connected tabs)
let currentExpiresAt: number | undefined;

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

	// Send the current session state to the newly connected tab
	if (currentExpiresAt) {
		port.postMessage({ command: 'sessionState', expiresAt: currentExpiresAt });
	}

	// Listen for messages from any port connected to this worker
	port.onmessage = (e: MessageEvent) => {
		if (e.data?.command === 'init') {
			if (e.data.expiresAt) {
				console.log('[Token Check Worker] Initializing with expiresAt:', e.data.expiresAt);
				currentExpiresAt = e.data.expiresAt;
				init(e.data.expiresAt);
			} else if (interval) {
				console.warn('[Token Check Worker] No expiresAt provided, stopping the interval.');
				clearInterval(interval);
				interval = undefined;
				currentExpiresAt = undefined;
			}
		}
	};
};

/**
 * Checks if the session has expired based on the absolute expiresAt timestamp.
 */
function isTokenExpired(expiresAt: number) {
	const currentTime = Math.floor(Date.now() / 1000);
	const secondsRemaining = expiresAt - currentTime;

	console.log('[Token Check Worker] Token expires in', secondsRemaining, 'seconds');

	const bufferedSeconds = Math.max(0, secondsRemaining - BUFFER_BEFORE_EXPIRATION);

	return {
		tokenIsExpired: bufferedSeconds === 0,
		numberOfSecondsUntilExpiration: bufferedSeconds,
	};
}

/**
 * Broadcasts a message to all connected ports, removing stale ones.
 */
function broadcastMessage(message: unknown) {
	for (let i = ports.length - 1; i >= 0; i--) {
		try {
			ports[i].postMessage(message);
		} catch {
			// Port is dead — remove it
			ports.splice(i, 1);
		}
	}
}

/**
 * This worker checks the token expiration at regular intervals.
 * If the token is expired or missing, it will trigger a logout.
 */
function init(expiresAt: number) {
	console.log('[Token Check Worker] Initializing token check worker...');

	if (interval) {
		clearInterval(interval);
	}

	interval = setInterval(() => {
		const result = isTokenExpired(expiresAt);

		if (result.tokenIsExpired) {
			console.log('[Token Check Worker] Token is expired or missing, triggering logout.');
			broadcastMessage({ command: 'logout' });
			clearInterval(interval);
			interval = undefined;
			console.log('[Token Check Worker] Waiting for token refresh...');
		} else if (result.numberOfSecondsUntilExpiration <= BUFFER_BEFORE_EXPIRATION) {
			console.log('[Token Check Worker] Token should be refreshed, but it is not expired yet.');
			broadcastMessage({
				command: 'refreshToken',
				secondsUntilLogout: result.numberOfSecondsUntilExpiration,
			});
		}
	}, VALIDATION_INTERVAL);
}
