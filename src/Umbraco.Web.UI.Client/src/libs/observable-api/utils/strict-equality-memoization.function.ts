/**
 * @function strictEqualityMemoization
 * @param {unknown} previousValue - The previous value to compare.
 * @param {unknown} currentValue - The current value to compare.
 * @returns {boolean} - Returns true if the values are identical.
 * @description - Compares the two values using strict equality.
 */
export function strictEqualityMemoization(previousValue: unknown, currentValue: unknown): boolean {
	return previousValue === currentValue;
}
