import type { DeepPartial } from '../type/deep-partial.type.js';

/**
 * Deep merge two objects.
 * @param target
 * @param ...sources
 */
export function umbDeepMerge<T extends { [key: string]: any }>(source: DeepPartial<T>, fallback: T) {
	const result = { ...fallback };

	for (const key in source) {
		if (Object.prototype.hasOwnProperty.call(source, key) && source[key] !== undefined) {
			if (source[key].constructor === Object && fallback[key].constructor === Object) {
				result[key] = umbDeepMerge(source[key], fallback[key]);
			} else {
				result[key] = source[key] as any;
			}
		}
	}

	return result;
}
