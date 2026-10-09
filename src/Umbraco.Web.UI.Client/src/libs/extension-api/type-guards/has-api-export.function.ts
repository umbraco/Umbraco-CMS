/**
 * Validate if an object has an 'api' export.
 * @param {unknown} object The object to check.
 * @returns {boolean} True if the object has an 'api' property.
 */
export function hasApiExport<ConstructorType>(object: unknown): object is { api: ConstructorType } {
	return typeof object === 'object' && object !== null && 'api' in object;
}
