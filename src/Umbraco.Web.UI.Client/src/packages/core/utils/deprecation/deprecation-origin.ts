/**
 * Where a deprecated API was most likely called from, derived from the call stack.
 * Best-effort and only used to annotate console warnings — never load-bearing.
 */
export interface UmbDeprecationOrigin {
	/**
	 * - `core`: Umbraco backoffice itself
	 * - `package`: an `/App_Plugins/` package
	 * - `external`: other non-core code (e.g. a Razor Class Library)
	 * - `unknown`: could not be determined (e.g. a development build or no stack)
	 */
	type: 'core' | 'package' | 'external' | 'unknown';
	/** Human-readable label for the console message. */
	label: string;
}

// In a production build, core is served from this path; any frame under it is core code.
const UMB_CORE_PATH_MARKER = '/umbraco/backoffice/';

/**
 * Classifies the caller of a deprecated API from a stack trace.
 *
 * The frames read: the deprecation util → the deprecated API (core) → … → the caller. So the first
 * frame that is *not* core code is the most likely caller; if every frame is core, the caller is core
 * itself. Detection only works for production builds where core is served under `/umbraco/backoffice/`
 * — in development builds it returns `unknown`.
 * @param {string | undefined} stack - A raw `Error.stack` string (the format differs per engine).
 * @param {string} selfUrl - The deprecation util's own module URL (`import.meta.url`), used both to recognise core and to drop its own frames.
 * @returns {UmbDeprecationOrigin} The classified origin.
 */
export function umbParseDeprecationOrigin(stack: string | undefined, selfUrl: string): UmbDeprecationOrigin {
	if (!stack) return { type: 'unknown', label: 'unknown' };

	// Frame URLs, tolerant of Chrome "at fn (URL:1:2)" and Firefox/Safari "fn@URL:1:2"; strip line:col and any query/fragment.
	const urls = Array.from(stack.matchAll(/(?:https?|blob|file):\/\/[^\s)'"]+/g)).map((match) =>
		match[0].replace(/:\d+:\d+\)?$/, '').replace(/[?#].*$/, ''),
	);

	const selfBase = selfUrl.replace(/[?#].*$/, '');
	const frames = urls.filter((url) => url !== selfBase);

	const coreKnown = selfUrl.includes(UMB_CORE_PATH_MARKER) || frames.some((url) => url.includes(UMB_CORE_PATH_MARKER));
	if (!coreKnown) return { type: 'unknown', label: 'unknown' };

	const caller = frames.find((url) => !url.includes(UMB_CORE_PATH_MARKER));
	if (!caller) return { type: 'core', label: 'Umbraco backoffice (core)' };

	const appPlugin = caller.match(/\/App_Plugins\/([^/?#]+)/i);
	if (appPlugin) return { type: 'package', label: `package "${appPlugin[1]}" (/App_Plugins/${appPlugin[1]})` };

	return { type: 'external', label: `custom code (${caller})` };
}

/**
 * Decides whether a deprecation should be logged.
 *
 * Core-origin deprecations are noise in production — a consumer cannot act on Umbraco's own code — so
 * they are suppressed there. Package, external and unknown origins are always logged so the responsible
 * developer sees them.
 * @param {UmbDeprecationOrigin} origin - The classified origin.
 * @param {boolean} suppressCore - Whether core-origin warnings should be suppressed (production).
 * @returns {boolean} `true` if the warning should be logged.
 */
export function umbShouldLogDeprecation(origin: UmbDeprecationOrigin, suppressCore: boolean): boolean {
	return !(suppressCore && origin.type === 'core');
}
