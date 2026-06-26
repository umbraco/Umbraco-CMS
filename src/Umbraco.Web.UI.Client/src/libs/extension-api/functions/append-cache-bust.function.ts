const APP_PLUGINS_PREFIX = '/app_plugins/';

/**
 * When `autoStamp` is on, appends `?v=<version>&umb__rnd=<cacheBuster>` (only the values present) to a clean
 * `/App_Plugins` URL. Existing-query, CDN, bare-specifier and backoffice-core URLs are untouched. (The
 * `%CACHE_BUSTER%` token is resolved server-side, so it never reaches here.)
 */
export function appendCacheBust(
	url: string,
	version: string | null | undefined,
	cacheBuster: string | null | undefined,
	autoStamp: boolean,
): string {
	if (!url || !autoStamp) {
		return url;
	}

	// Only auto-stamp the package's own /App_Plugins assets, and never a URL that already manages its own query.
	if (!url.toLowerCase().startsWith(APP_PLUGINS_PREFIX) || url.includes('?')) {
		return url;
	}

	const params: string[] = [];
	if (version) {
		params.push(`v=${encodeURIComponent(version)}`);
	}
	if (cacheBuster) {
		params.push(`umb__rnd=${encodeURIComponent(cacheBuster)}`);
	}
	if (params.length === 0) {
		return url;
	}

	const query = params.join('&');
	const hashIndex = url.indexOf('#');
	return hashIndex < 0 ? `${url}?${query}` : `${url.slice(0, hashIndex)}?${query}${url.slice(hashIndex)}`;
}
