/**
 *
 * @param target
 * @param source
 */
export function assignToFrozenObject<T extends object>(target: T, source: Partial<T>): T {
	return Object.assign(Object.create(Object.getPrototypeOf(target)), target, source);
}
