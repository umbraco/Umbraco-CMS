import { jsonStringComparison } from './json-string-comparison.function.js';

/**
 * @function defaultMemoization
 * @param {any} previousValue - The previous value to compare.
 * @param {any} currentValue - The current value to compare.
 * @returns {boolean} - Returns true if the values are identical.
 * @description - Default memoization function to compare two values.
 * This checks if the two values are of type 'object' (Array or Object) and compares them using a naive JSON string comparison.
 * If not then it compares the two values using strict equality.
 */
export function defaultMemoization(previousValue: any, currentValue: any): boolean {
	if (typeof previousValue === 'object' && typeof currentValue === 'object') {
		return jsonStringComparison(previousValue, currentValue);
	}
	return previousValue === currentValue;
}
