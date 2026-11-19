/**
 * Ensure that the path is a local path.
 * @param {URL | string} path - The path to check.
 * @param {URL | string} [fallbackPath] - The path to use if the given path is not local.
 * @returns {URL} The local path, or the fallback path if the given path is not local.
 */
export function ensureLocalPath(path: URL | string, fallbackPath?: URL | string): URL {
	const url = new URL(path, window.location.origin);
	if (url.origin === window.location.origin) {
		return url;
	}
	return fallbackPath ? new URL(fallbackPath) : new URL(window.location.origin);
}
