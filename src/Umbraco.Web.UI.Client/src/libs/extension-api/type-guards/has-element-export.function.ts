/**
 *
 * @param object
 */
export function hasElementExport<ConstructorType>(object: unknown): object is { element: ConstructorType } {
	return typeof object === 'object' && object !== null && 'element' in object;
}
