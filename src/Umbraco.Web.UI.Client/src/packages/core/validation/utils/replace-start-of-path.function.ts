/**
 * Helper method to replace the start of a JSON Path with another JSON Path.
 * @param {string} path - the JSON path to transform.
 * @param {string} startFrom - the path prefix to match against.
 * @param {string} startTo - the path prefix to replace it with.
 * @returns {string} the transformed path, or undefined if `path` does not start with `startFrom`.
 */
export function ReplaceStartOfPath(path: string, startFrom: string, startTo: string): string | undefined {
	// if the path continues with a . after startFrom, then replace it with startTo, otherwise if identical then it is also a match. [NL]
	if (path.startsWith(startFrom + '.') || path === startFrom) {
		return startTo + path.slice(startFrom.length);
	}
	return;
}
