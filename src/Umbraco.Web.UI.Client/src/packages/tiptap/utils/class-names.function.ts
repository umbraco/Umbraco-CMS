/**
 * Splits a whitespace-separated class attribute value into its individual class names.
 * @param {unknown} value The class attribute value, e.g. from a node or mark's attributes.
 * @returns {Array<string>} The individual class names, in order, with no empty entries.
 */
export function splitClassNames(value: unknown): Array<string> {
	return String(value ?? '')
		.split(/\s+/)
		.filter((className) => className);
}

/**
 * Checks whether every class name in `classNames` is present in `value`, comparing whole class
 * names rather than substrings (so e.g. `size-1` does not match `size-10`).
 * @param {unknown} value The class attribute value to check against.
 * @param {string} classNames One or more whitespace-separated class names that must all be present.
 * @returns {boolean} Returns true if every class name in `classNames` is present in `value`.
 */
export function hasClassNames(value: unknown, classNames: string): boolean {
	const classes = splitClassNames(value);
	return splitClassNames(classNames).every((className) => classes.includes(className));
}
