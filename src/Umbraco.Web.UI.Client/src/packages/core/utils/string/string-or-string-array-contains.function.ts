export function stringOrStringArrayContains(value: string | Array<string>, search: string) {
	return Array.isArray(value) ? value.indexOf(search) !== -1 : value === search;
}
