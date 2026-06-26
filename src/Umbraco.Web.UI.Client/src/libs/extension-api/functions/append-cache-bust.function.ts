const APP_PLUGINS_PREFIX = '/app_plugins/';
const CACHE_BUSTER_TOKEN = '%CACHE_BUSTER%';

/**
 * Applies cache-busting to a single package asset URL.
 *
 * - An explicit `%CACHE_BUSTER%` token is always resolved (wherever it appears) to the package `version`, falling
 *   back to the host `cacheBuster` — that is the author's deliberate opt-in.
 * - Otherwise, when `autoStamp` is `true`, a clean `/App_Plugins`-rooted URL gets
 *   `?v=<version>&umb__rnd=<cacheBuster>` appended (only the values that are present).
 * - Everything else — the backoffice core, CDNs, protocol-relative URLs, bare module specifiers, and URLs that
 *   already carry a query string — is returned unchanged.
 */
export function appendCacheBust(
	url: string,
	version: string | null | undefined,
	cacheBuster: string | null | undefined,
	autoStamp: boolean,
): string {
	if (!url) {
		return url;
	}

	// The explicit %CACHE_BUSTER% opt-in: resolve it wherever the author placed it, regardless of autoStamp.
	if (url.includes(CACHE_BUSTER_TOKEN)) {
		return url.replaceAll(CACHE_BUSTER_TOKEN, version || cacheBuster || '');
	}

	if (!autoStamp) {
		return url;
	}

	// Automatic stamping only ever touches the package's own /App_Plugins assets (case-insensitive). This also
	// excludes the backoffice core, CDNs, protocol-relative URLs, bare specifiers and relative paths.
	if (!url.toLowerCase().startsWith(APP_PLUGINS_PREFIX)) {
		return url;
	}

	// The author already manages this URL's query — leave it alone.
	if (url.includes('?')) {
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
