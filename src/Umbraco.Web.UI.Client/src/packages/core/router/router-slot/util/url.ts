import type { ISlashOptions, Query } from '../model.js';

const $anchor = document.createElement('a');

/**
 * The current path of the location.
 * As default slashes are included at the start and end.
 * @param {Partial<ISlashOptions>} options - The slash options
 * @returns {string} The current path
 */
export function path(options: Partial<ISlashOptions> = {}): string {
	return slashify(window.location.pathname, options);
}

/**
 * Returns the path without the base path.
 * @param {Partial<ISlashOptions>} options - The slash options
 * @returns {string} The path without the base path
 */
export function pathWithoutBasePath(options: Partial<ISlashOptions> = {}): string {
	return slashify(stripStart(path(), basePath()), options);
}

/**
 * Returns the base path as defined in the <base> tag in the head in a reliable way.
 * If eg. <base href="/router-slot/"> is defined this function will return "/router-slot/".
 *
 * An alternative would be to use regex on document.baseURI,
 * but that will be unreliable in some cases because it
 * doesn't use the built in HTMLHyperlinkElementUtils.
 *
 * To make this method more performant we could cache the anchor element.
 * As default it will return the base path with slashes in front and at the end.
 * @param {Partial<ISlashOptions>} options - The slash options
 * @returns {string} The base path
 */
export function basePath(options: Partial<ISlashOptions> = {}): string {
	return constructPathWithBasePath('.', options);
}

/**
 * Creates an URL using the built in HTMLHyperlinkElementUtils.
 * An alternative would be to use regex on document.baseURI,
 * but that will be unreliable in some cases because it
 * doesn't use the built in HTMLHyperlinkElementUtils.
 *
 * As default it will return the base path with slashes in front and at the end.
 * @param {string} path - The path to resolve relative to the base path
 * @param {Partial<ISlashOptions>} options - The slash options
 * @returns {string} The resolved path
 */
export function constructPathWithBasePath(path: string, options: Partial<ISlashOptions> = {}) {
	$anchor.href = path;
	return slashify($anchor.pathname, options);
}

/**
 * Removes the start of the path that matches the part.
 * @param {string} path - The path to strip
 * @param {string} part - The part to remove from the start
 * @returns {string} The path without the matching start
 */
export function stripStart(path: string, part: string) {
	return path.replace(new RegExp(`^${part}`), '');
}

/**
 * Returns the query string.
 * @returns {string} The query string
 */
export function queryString(): string {
	return window.location.search;
}

/**
 * Returns the params for the current path.
 * @returns {Query} Params
 */
export function query(): Query {
	return toQuery(queryString().substring(1));
}

/**
 * Strips the slash from the start and end of a path.
 * @param {string} path - The path to strip
 * @returns {string} The path without leading/trailing slashes
 */
export function stripSlash(path: string): string {
	return slashify(path, { start: false, end: false });
}

/**
 * Ensures the path starts and ends with a slash
 * @param {string} path - The path to ensure slashes on
 * @returns {string} The path with leading and trailing slashes
 */
export function ensureSlash(path: string): string {
	return slashify(path, { start: true, end: true });
}

/**
 * Makes sure that the start and end slashes are present or not depending on the options.
 * @param {string} path - The path to slashify
 * @param {Partial<ISlashOptions>} options - The slash options
 * @param {boolean} [options.start] - Whether the path should start with a slash
 * @param {boolean} [options.end] - Whether the path should end with a slash
 * @returns {string} The slashified path
 */
export function slashify(path: string, { start = true, end = true }: Partial<ISlashOptions> = {}): string {
	path = start && !path.startsWith('/') ? `/${path}` : !start && path.startsWith('/') ? path.slice(1) : path;
	return end && !path.endsWith('/') ? `${path}/` : !end && path.endsWith('/') ? path.slice(0, path.length - 1) : path;
}

/**
 * Turns a query string into an object.
 * @param {string} queryString (example: ("test=123&hejsa=LOL&wuhuu"))
 * @returns {Query} The parsed query object
 */
export function toQuery(queryString: string): Query {
	// If the query does not contain anything, return an empty object.
	if (queryString.length === 0) {
		return {};
	}

	// Grab the atoms (["test=123", "hejsa=LOL", "wuhuu"])
	const atoms = queryString.split('&');

	// Split by the values ([["test", "123"], ["hejsa", "LOL"], ["wuhuu"]])
	const arrayMap = atoms.map((atom) => atom.split('='));

	// Assign the values to an object ({ test: "123", hejsa: "LOL", wuhuu: "" })
	return Object.assign(
		{},
		...arrayMap.map((arr) => ({
			[decodeURIComponent(arr[0])]: arr.length > 1 ? decodeURIComponent(arr[1]) : '',
		})),
	);
}

/**
 * Turns a query object into a string query.
 * @param {Query} query - The query object to stringify
 * @returns {string} The query string
 */
export function toQueryString(query: Query): string {
	return Object.entries(query)
		.map(([key, value]) => `${key}${value != '' ? `=${encodeURIComponent(value)}` : ''}`)
		.join('&');
}
