/**
 * @description Check if a string or array of strings contains a specific string
 * @param value The string or array of strings to search in
 * @param search The string to search for
 * @returns {boolean} Whether the string or array of strings contains the search string
 */
export function stringOrStringArrayContains(value: string | Array<string>, search: string): boolean {
	return Array.isArray(value) ? value.indexOf(search) !== -1 : value === search;
}
