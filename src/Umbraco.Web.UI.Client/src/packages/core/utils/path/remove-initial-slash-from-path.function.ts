/**
 * Removes the initial slash from a path, if the first character is a slash.
 * @param path
 */
export function removeInitialSlashFromPath(path: string) {
	return path.startsWith('/') ? path.slice(1) : path;
}
