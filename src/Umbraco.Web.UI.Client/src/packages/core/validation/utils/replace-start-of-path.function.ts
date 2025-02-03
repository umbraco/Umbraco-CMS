/**
 * Helper method to replace the start of a JSON Path with another JSON Path.
 * @param path {string}
 * @param startFrom {string}
 * @param startTo {string}
 * @returns {string}
 */
export function ReplaceStartOfPath(path: string, startFrom: string, startTo: string): string | undefined {
	// if the path conitnues with a . or [ aftr startFrom, then replace it with startTo, otherwise if identical then it is also a match. [NL]
	if (path.startsWith(startFrom + '.') || path === startFrom) {
		return startTo + path.slice(startFrom.length);
	}
	return;
}
