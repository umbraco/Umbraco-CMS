/**
 * Removes the initial slash from a path, if the first character is a slash.
 * @param {string} path - The path to remove the initial slash from.
 * @returns {string} The path without an initial slash.
 */
export function removeInitialSlashFromPath(path: string) {
	return path.startsWith('/') ? path.slice(1) : path;
}
