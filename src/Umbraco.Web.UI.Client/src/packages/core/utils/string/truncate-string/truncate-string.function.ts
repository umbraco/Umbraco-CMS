/**
 * Shortens a string to a maximum length by replacing its middle with a separator, keeping the start and the end.
 * @example truncateString('A very long document name', 15) // "A very…nt name"
 * @param {string | undefined} input The string to shorten.
 * @param {number} maxLength The maximum length of the result, including the separator.
 * @param {string} [separator] The text that replaces the removed characters.
 * @returns {string} The input if it fits within `maxLength`, otherwise the shortened string. An empty string if no input is given.
 */
export function truncateString(input: string | undefined, maxLength: number, separator = '…'): string {
	if (!input) return '';
	if (input.length <= maxLength) return input;

	const charsToShow = Math.max(maxLength - separator.length, 0);
	const frontChars = Math.ceil(charsToShow / 2);
	const backChars = Math.floor(charsToShow / 2);

	return input.substring(0, frontChars) + separator + input.substring(input.length - backChars);
}
