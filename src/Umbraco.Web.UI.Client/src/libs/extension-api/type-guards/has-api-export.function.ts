/**
 *
 * @param object
 */
export function hasApiExport<ConstructorType>(object: unknown): object is { api: ConstructorType } {
	return typeof object === 'object' && object !== null && 'api' in object;
}
