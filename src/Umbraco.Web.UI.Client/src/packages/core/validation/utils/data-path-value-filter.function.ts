import type { UmbVariantableValueModel } from '@umbraco-cms/backoffice/models';

/**
 * write a JSON-Path filter similar to `?(@.alias = 'myAlias' && @.culture == 'en-us' && @.segment == 'mySegment')`
 * where culture and segment are optional
 * @param property
 * @returns
 */
export function UmbDataPathValueFilter(property: Partial<UmbVariantableValueModel>): string {
	// write a array of strings for each property, where alias must be present and culture and segment are optional
	const filters: Array<string> = [`@.alias = '${property.alias}'`];
	if (property.culture) {
		filters.push(`@.culture == '${property.culture}'`);
	}
	if (property.segment) {
		filters.push(`@.segment == '${property.segment}'`);
	}
	return `?(${filters.join(' && ')})`;
}
