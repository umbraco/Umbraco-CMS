/**
 * Ensure that the path is a local path.
 * @param path
 * @param fallbackPath
 */
export function ensureLocalPath(path: URL | string, fallbackPath?: URL | string): URL {
	const url = new URL(path, window.location.origin);
	if (url.origin === window.location.origin) {
		return url;
	}
	return fallbackPath ? new URL(fallbackPath) : new URL(window.location.origin);
}
