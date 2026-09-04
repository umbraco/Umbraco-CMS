/**
 * Determines whether a collection holds fewer items than the configured minimum.
 *
 * A minimum applies only to a collection that is in use. Whether an empty collection is acceptable is decided by
 * whether the property is mandatory, so that configuring a data type can never make an optional property required.
 * @param {number} count - the number of items held.
 * @param {number | undefined} min - the configured minimum, where undefined or zero means no minimum.
 * @returns {boolean} true when the minimum applies and is not met.
 */
export function isBelowMinItemCount(count: number, min: number | undefined): boolean {
	return !!min && count > 0 && count < min;
}
