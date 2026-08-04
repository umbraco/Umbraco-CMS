/**
 * Resolves a configured stylesheet path to an absolute href on the Umbraco server.
 * @param {string} stylesheet The configured stylesheet path, relative to `rootPath`.
 * @param {string} rootPath The stylesheet root path reported by the server, e.g. `/css`.
 * @param {string} serverUrl The Umbraco server origin. Falls back to `window.location.origin` if empty.
 * @returns {string} The resolved href to use for the stylesheet link.
 */
export function resolveStylesheetHref(stylesheet: string, rootPath: string, serverUrl: string): string {
	if (stylesheet.startsWith('http') || stylesheet.startsWith('//')) return stylesheet;
	const relativeHref = stylesheet.startsWith(rootPath) ? stylesheet : `${rootPath}${stylesheet}`;
	return new URL(relativeHref, serverUrl || window.location.origin).href;
}
