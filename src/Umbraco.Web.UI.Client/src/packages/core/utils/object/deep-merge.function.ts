import type { UmbDeepPartialObject } from '../type/deep-partial-object.type.js';

/**
 * Deep merge two objects.
 * @template {{ [key: string]: unknown }} T
 * @template {UmbDeepPartialObject<T>} PartialType
 * @param {PartialType} source - The partial object to merge into the fallback.
 * @param {T} fallback - The object providing the default values.
 * @returns {T} The merged object.
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
