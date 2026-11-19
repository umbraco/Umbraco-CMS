/**
 * Validate if an object has a 'default' export.
 * @param {unknown} object The object to check.
 * @returns {boolean} True if the object has a 'default' property.
 */
export function hasDefaultExport<ConstructorType>(object: unknown): object is { default: ConstructorType } {
	return typeof object === 'object' && object !== null && 'default' in object;
}
