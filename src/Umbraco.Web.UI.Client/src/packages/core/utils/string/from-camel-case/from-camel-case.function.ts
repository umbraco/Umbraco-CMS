/**
 * Converts a string from camelCase to human-readable labels.
 *
 * This function has been adapted from the following Stack Overflow answer:
 * https://stackoverflow.com/a/7225450/12787
 * Licensed under the permissions of the CC BY-SA 4.0 DEED.
 * https://creativecommons.org/licenses/by-sa/4.0/
 * Modifications are licensed under the MIT License.
 * Copyright © 2024 Umbraco HQ.
 * @param {string} str - The camelCased string to convert.
 * @returns {string} - The converted human-readable label.
 * @example
 * const label = fromCamelCase('workspaceActionMenuItem');
 * // label: 'Workspace Action Menu Item'
 */
export const fromCamelCase = (str: string) => {
	const s = str.replace(/([A-Z])/g, ' $1');
	return s.charAt(0).toUpperCase() + s.slice(1);
};

/**
 * Applies {@link fromCamelCase} only when the string looks like camelCase
 * (starts with a lowercase letter and contains no spaces).
 * Strings that are already display-ready (PascalCase, contain spaces, etc.)
 * are returned as-is.
 * @param {string} str - The string to conditionally convert.
 * @returns {string} - The converted or original string.
 */
export const fromCamelCaseIfCamelCase = (str: string) => {
	if (str.includes(' ') || str.charAt(0) === str.charAt(0).toUpperCase()) {
		return str;
	}
	return fromCamelCase(str);
};
