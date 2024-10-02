/**
 * @description Increment string
 * @param {string} text The text to increment
 * @returns {string} The incremented string
 */
export function incrementString(text: string): string {
	return text.replace(/(\d*)$/, (_, t) => (+t + 1).toString().padStart(t.length, '0'));
}
