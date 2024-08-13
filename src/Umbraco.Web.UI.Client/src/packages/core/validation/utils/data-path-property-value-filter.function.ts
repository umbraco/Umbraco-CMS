import type { UmbPartialSome } from '@umbraco-cms/backoffice/utils';
import type { UmbVariantPropertyValueModel } from '@umbraco-cms/backoffice/variant';

/**
 * Validation Data Path filter for Property Value.
 * write a JSON-Path filter similar to `?(@.alias = 'myAlias' && @.culture == 'en-us' && @.segment == 'mySegment')`
 * where culture and segment are optional
 * @param value
 * @returns
 */
export function UmbDataPathPropertyValueFilter(
	value: UmbPartialSome<Omit<UmbVariantPropertyValueModel, 'value'>, 'culture' | 'segment'>,
): string {
	// write a array of strings for each property, where alias must be present and culture and segment are optional
	const filters: Array<string> = [`@.alias = '${value.alias}'`];
	if (value.culture) {
		filters.push(`@.culture = '${value.culture}'`);
	}
	if (value.segment) {
		filters.push(`@.segment = '${value.segment}'`);
	}
	return `?(${filters.join(' && ')})`;
}
