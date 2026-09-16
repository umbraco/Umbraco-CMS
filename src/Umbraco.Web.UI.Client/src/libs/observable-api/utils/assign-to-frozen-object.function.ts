/**
 *
 * @template {object} T
 * @param {T} target - The frozen object to base the new object on.
 * @param {Partial<T>} source - The properties to assign onto the new object.
 * @returns {T} - A new object, with the same prototype as `target`, with `source` assigned onto it.
 */
export function assignToFrozenObject<T extends object>(target: T, source: Partial<T>): T {
	return Object.assign(Object.create(Object.getPrototypeOf(target)), target, source);
}
