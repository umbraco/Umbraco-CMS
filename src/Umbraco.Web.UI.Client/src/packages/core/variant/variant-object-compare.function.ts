import type { UmbObjectWithVariantProperties } from './types.js';

/**
 *
 * @param a
 * @param b
 */
export function umbVariantObjectCompare(a: UmbObjectWithVariantProperties, b: UmbObjectWithVariantProperties): boolean {
	return a.culture === b.culture && a.segment === b.segment;
}
