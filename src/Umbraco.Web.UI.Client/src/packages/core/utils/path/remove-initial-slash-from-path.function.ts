/**
 *
 * @param path
 */
export function removeInitialSlashFromPath(path: string) {
	return path.startsWith('/') ? path.slice(1) : path;
}
