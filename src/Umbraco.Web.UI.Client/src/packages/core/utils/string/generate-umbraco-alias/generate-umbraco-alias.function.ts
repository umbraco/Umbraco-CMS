import { toCamelCase } from '../to-camel-case/index.js';

/**
 * @description Generate an alias from a string
 * @param {string} text The text to generate the alias from
 * @returns {string} The alias
 */
export function generateAlias(text: string): string {
	return toCamelCase(text);
}
