/* eslint-disable */
/* tslint:disable */

/**
 * Umbraco Service Worker Wrapper
 *
 * This wrapper intercepts fetch events BEFORE they reach MSW's service worker.
 * Static assets (JS/TS modules, CSS, fonts, images, etc.) are bypassed entirely,
 * avoiding the expensive round-trip to the main thread that MSW performs for
 * every intercepted request.
 *
 * This significantly improves startup performance when running with mocks in Vite,
 * as thousands of module requests no longer need to go through MSW's interception.
 * API requests to the same origin are still handled by MSW as expected.
 * https://github.com/mswjs/msw/discussions/1424
 */

// Static asset extensions to bypass
const BYPASS_EXTENSIONS = new Set([
	'.js',
	'.mjs',
	'.ts',
	'.tsx',
	'.jsx',
	'.css',
	'.scss',
	'.sass',
	'.less',
	'.woff',
	'.woff2',
	'.ttf',
	'.otf',
	'.eot',
	'.png',
	'.jpg',
	'.jpeg',
	'.gif',
	'.svg',
	'.webp',
	'.ico',
	'.avif',
	'.mp4',
	'.webm',
	'.ogg',
	'.mp3',
	'.wav',
	'.pdf',
	'.json',
	'.map',
]);

// Paths that indicate static assets/modules (Vite-specific)
const BYPASS_PATH_PATTERNS = [
	'/node_modules/',
	'/@vite/',
	'/@id/',
	'/@fs/',
	'/__vite_ping',
	'/src/', // Vite serves source files directly in dev mode
];

/**
 * Check if a request should bypass MSW entirely.
 * @param {Request} request
 * @returns {boolean}
 */
function shouldBypass(request) {
	const url = new URL(request.url);

	// Only bypass same-origin requests (don't interfere with API calls to other origins)
	if (url.origin !== self.location.origin) {
		return false;
	}

	// Check for Vite-specific paths
	for (const pattern of BYPASS_PATH_PATTERNS) {
		if (url.pathname.includes(pattern)) {
			return true;
		}
	}

	// Check file extension
	const lastDotIndex = url.pathname.lastIndexOf('.');
	if (lastDotIndex !== -1) {
		const extension = url.pathname.slice(lastDotIndex).toLowerCase();
		// Also handle query strings (e.g., .ts?t=123456)
		const cleanExtension = extension.split('?')[0];
		if (BYPASS_EXTENSIONS.has(cleanExtension)) {
			return true;
		}
	}

	return false;
}

// Register the bypass handler BEFORE importing MSW's worker
// Using addEventListener ensures this runs before MSW's handler
self.addEventListener('fetch', function (event) {
	if (shouldBypass(event.request)) {
		// Stop propagation to prevent MSW from handling this request
		event.stopImmediatePropagation();
		// Let the browser handle the request normally (no respondWith = passthrough)
		return;
	}
});

// Import the actual MSW service worker
// This must come AFTER our fetch listener so ours runs first
importScripts('./mockServiceWorker.js');
