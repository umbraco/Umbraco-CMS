/**
 * Extracts the contents of the first pair of square brackets in a JSON-Path string — the query of the
 * outermost/first array-item scope, ignoring anything nested deeper in the path.
 * @param {string} path - The JSON-Path string.
 * @returns {string | undefined} The bracket contents, or undefined if the path has no brackets.
 * @example
 * ```ts
 * umbGetFirstJsonPathBracket("$.values[?(@.alias == 'x')].value.contentData[?(@.key == 'y')]");
 * // => "?(@.alias == 'x')"
 * ```
 */
export function umbGetFirstJsonPathBracket(path: string): string | undefined {
	const start = path.indexOf('[');
	if (start === -1) return undefined;

	const end = path.indexOf(']', start + 1);
	if (end === -1) return undefined;

	return path.substring(start + 1, end);
}
