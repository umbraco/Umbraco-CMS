/**
 *
 * @param object
 */
export function hasDefaultExport<ConstructorType>(object: unknown): object is { default: ConstructorType } {
	return typeof object === 'object' && object !== null && 'default' in object;
}
