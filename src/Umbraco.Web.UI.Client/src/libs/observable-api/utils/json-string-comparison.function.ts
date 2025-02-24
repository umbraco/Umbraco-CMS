/**
 * @function jsonStringComparison
 * @param {object | undefined} a - The first object to compare.
 * @param {object | undefined} b - The second object to compare.
 * @returns {boolean} - Returns true if the JSON strings are identical.
 * @description - Compares two objects by converting them to JSON strings.
 * This is a JSON comparison and should only be used for simple objects.
 * Meaning no class instances can take part in this data.
 */
export function jsonStringComparison(a: object | undefined, b: object | undefined): boolean {
	return JSON.stringify(a) === JSON.stringify(b);
}
