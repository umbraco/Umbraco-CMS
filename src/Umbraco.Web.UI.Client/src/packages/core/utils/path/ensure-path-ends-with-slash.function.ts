/**
 * Ensure that the path ends with a slash.
 * @param {string} path - The path to check.
 * @returns {string} The path, ending with a slash.
 */
export function ensurePathEndsWithSlash(path: string) {
	return path.endsWith('/') ? path : path + '/';
}
