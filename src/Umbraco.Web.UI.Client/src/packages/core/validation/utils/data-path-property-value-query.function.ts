import type { UmbPartialSome } from '@umbraco-cms/backoffice/utils';
import type { UmbVariantPropertyValueModel } from '@umbraco-cms/backoffice/variant';

/**
 * Validation Data Path Query generator for Property Value.
 * write a JSON-Path filter similar to `?(@.alias == 'myAlias' && @.culture == 'en-us' && @.segment == 'mySegment')`
 * where culture and segment are optional
 * @param {UmbVariantPropertyValueModel} value - the object holding value and alias.
 * @returns {string} - a JSON-path query
 */
export function UmbDataPathPropertyValueQuery(
	value: UmbPartialSome<Omit<UmbVariantPropertyValueModel, 'value'>, 'culture' | 'segment'>,
): string {
	// write a array of strings for each property, where alias must be present and culture and segment are optional
	const filters: Array<string> = [`@.alias == '${value.alias}'`];
	if (value.culture !== undefined) {
		filters.push(`@.culture == ${value.culture ? `'${value.culture}'` : 'null'}`);
	}
	if (value.segment !== undefined) {
		filters.push(`@.segment == ${value.segment ? `'${value.segment}'` : 'null'}`);
	}
	return `?(${filters.join(' && ')})`;
}
