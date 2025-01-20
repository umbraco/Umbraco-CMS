/**
 * @description Check if a string or array of strings contains a specific string
 * @param value The string or array of strings to search in
 * @param search The string to search for
 * @returns {boolean} Whether the string or array of strings contains the search string
 */
export function stringOrStringArrayContains(value: string | Array<string>, search: string): boolean {
	return Array.isArray(value) ? value.indexOf(search) !== -1 : value === search;
}

/**
 * Check if a string or array of strings intersects with another array of strings
 * @param value The string or array of strings to search in
 * @param search The array of strings to search for
 * @returns {boolean} Whether the string or array of strings intersects with the search array
 */
export function stringOrStringArrayIntersects(value: string | Array<string>, search: Array<string>): boolean {
	if (Array.isArray(value)) {
		return value.some((v) => search.indexOf(v) !== -1);
	} else {
		return search.indexOf(value) !== -1;
	}
}
