/**
 * Splits a string into an array using a specified delimiter,
 * trims whitespace from each element, and removes empty elements.
 * @param {string | undefined} string - The input string to be split and processed.
 * @param {string} [split] - The delimiter used for splitting the string (default is comma).
 * @returns {Array<string>} An array of non-empty, trimmed strings.
 * @example
 * const result = splitStringToArray('one, two, three, ,five');
 * // result: ['one', 'two', 'three', 'five']
 * @example
 * const customDelimiterResult = splitStringToArray('apple | orange | banana', ' | ');
 * // customDelimiterResult: ['apple', 'orange', 'banana']
 */
export function splitStringToArray(string: string | undefined, split: string = ','): string[] {
	if (!string) return [];
	return (
		string
			.split(split)
			.map((s) => s.trim())
			.filter((s) => s.length > 0) ?? []
	);
}
