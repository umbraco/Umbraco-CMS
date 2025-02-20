type StringMaybeUndefined = string | undefined;

/**
 *
 * @param path
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
