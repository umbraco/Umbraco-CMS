import type { UmbDeepPartialObject } from '../type/deep-partial-object.type.js';

/**
 * Deep merge two objects.
 * @param target
 * @param ...sources
 * @param source
 * @param fallback
 */
export function umbDeepMerge<
	T extends { [key: string]: any },
	PartialType extends UmbDeepPartialObject<T> = UmbDeepPartialObject<T>,
>(source: PartialType, fallback: T) {
	const result = { ...fallback };

	for (const key in source) {
		if (Object.prototype.hasOwnProperty.call(source, key) && source[key] !== undefined) {
			if (source[key]?.constructor === Object && fallback[key]?.constructor === Object) {
				result[key] = umbDeepMerge(source[key] as any, fallback[key]);
			} else {
				result[key] = source[key] as any;
			}
		}
	}

	return result;
}
