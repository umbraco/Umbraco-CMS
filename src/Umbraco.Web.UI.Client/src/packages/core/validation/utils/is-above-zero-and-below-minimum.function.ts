/**
 * Determines whether a count is above zero and below the configured minimum.
 * @param {number} count - the number of items held.
 * @param {number | undefined} min - the configured minimum, where undefined or zero means no minimum.
 * @returns {boolean} true when the count is above zero and below the minimum.
 */
export function isAboveZeroAndBelowMinimum(count: number, min: number | undefined): boolean {
	return !!min && count > 0 && count < min;
}
