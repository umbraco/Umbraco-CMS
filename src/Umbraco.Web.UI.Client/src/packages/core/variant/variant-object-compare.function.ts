import type { UmbObjectWithVariantProperties } from './types.js';

export function umbVariantObjectCompare(a: UmbObjectWithVariantProperties, b: UmbObjectWithVariantProperties): boolean {
	return a.culture === b.culture && a.segment === b.segment;
}
