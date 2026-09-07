import type { UmbObjectWithVariantProperties } from './types.js';

/**
 * Compares the culture and segment of two variant objects.
 * @param {UmbObjectWithVariantProperties} a - the first object to compare.
 * @param {UmbObjectWithVariantProperties} b - the second object to compare.
 * @returns {boolean} true if both objects share the same culture and segment.
 */
export function umbVariantObjectCompare(a: UmbObjectWithVariantProperties, b: UmbObjectWithVariantProperties): boolean {
	return a.culture === b.culture && a.segment === b.segment;
}
