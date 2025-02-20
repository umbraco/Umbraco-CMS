/**
 * Remove the last slash from a path, if the last character is a slash.
 * @param path
 */
export function removeLastSlashFromPath(path: string) {
	return path.endsWith('/') ? path.slice(undefined, -1) : path;
}
