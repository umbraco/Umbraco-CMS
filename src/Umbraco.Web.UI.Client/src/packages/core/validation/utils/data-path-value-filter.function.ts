import type { UmbVariantableValueModel } from '@umbraco-cms/backoffice/models';

/**
 * write a JSON-Path filter similar to `?(@.alias = 'myAlias' && @.culture == 'en-us' && @.segment == 'mySegment')`
 * where culture and segment are optional
 * @param value
 * @returns
 */
export function UmbDataPathValueFilter(value: Omit<UmbVariantableValueModel, 'value'>): string {
	// write a array of strings for each property, where alias must be present and culture and segment are optional
	const filters: Array<string> = [`@.alias = '${value.alias}'`];
	if (value.culture) {
		filters.push(`@.culture == '${value.culture}'`);
	}
	if (value.segment) {
		filters.push(`@.segment == '${value.segment}'`);
	}
	return `?(${filters.join(' && ')})`;
}
