/**
 * Validate if an object has an 'element' export.
 * @param {unknown} object The object to check.
 * @returns {boolean} True if the object has an 'element' property.
 */
export function hasElementExport<ConstructorType>(object: unknown): object is { element: ConstructorType } {
	return typeof object === 'object' && object !== null && 'element' in object;
}
