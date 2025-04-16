/**
 *
 * @param path
 */
export function ensurePathEndsWithSlash(path: string) {
	return path.endsWith('/') ? path : path + '/';
}
