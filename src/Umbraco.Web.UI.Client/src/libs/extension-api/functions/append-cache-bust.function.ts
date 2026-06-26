const APP_PLUGINS_PREFIX = '/app_plugins/';

/**
 * Appends the server-computed `?umb__rnd=<cacheBuster>` to a clean `/App_Plugins` URL. URLs outside `/App_Plugins`,
 * URLs that already carry a query string, and an empty `cacheBuster` are returned unchanged.
 * @param {string} url The package asset URL (its own clean `/App_Plugins` path).
 * @param {string | null | undefined} cacheBuster The server-computed cache-bust value, or empty to skip stamping.
 * @returns {string} The stamped URL, or the original URL when no stamping applies.
 */
export function appendCacheBust(url: string, cacheBuster: string | null | undefined): string {
	if (!url || !cacheBuster) {
		return url;
	}

	// Only stamp the package's own /App_Plugins assets, and never a URL that already manages its own query.
	if (!url.toLowerCase().startsWith(APP_PLUGINS_PREFIX) || url.includes('?')) {
		return url;
	}

	const query = `umb__rnd=${encodeURIComponent(cacheBuster)}`;
	const hashIndex = url.indexOf('#');
	return hashIndex < 0 ? `${url}?${query}` : `${url.slice(0, hashIndex)}?${query}${url.slice(hashIndex)}`;
}
