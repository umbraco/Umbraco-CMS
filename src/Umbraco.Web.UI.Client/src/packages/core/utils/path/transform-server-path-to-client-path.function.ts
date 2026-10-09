type StringMaybeUndefined = string | undefined;

/**
 * Transforms a server-side path (e.g. `~/` or `/wwwroot/`) into a client-side path.
 * @template T
 * @param {T} path - The server-side path to transform.
 * @returns {T} The client-side path.
 */
export function transformServerPathToClientPath<T extends StringMaybeUndefined>(path: T): T {
	if (path?.indexOf('~/') === 0) {
		path = path.slice(1) as T;
	}
	if (path?.indexOf('/wwwroot/') === 0) {
		path = path.slice(8) as T;
	}
	return path;
}
