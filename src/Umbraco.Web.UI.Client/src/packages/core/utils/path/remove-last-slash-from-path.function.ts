/**
 * Remove the last slash from a path, if the last character is a slash.
 * @param {string} path - The path to remove the last slash from.
 * @returns {string} The path without a trailing slash.
 */
export function removeLastSlashFromPath(path: string) {
	return path.endsWith('/') ? path.slice(undefined, -1) : path;
}
