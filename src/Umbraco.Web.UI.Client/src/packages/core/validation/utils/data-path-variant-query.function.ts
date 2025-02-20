import type { UmbPartialSome } from '@umbraco-cms/backoffice/utils';
import type { UmbVariantPropertyValueModel } from '@umbraco-cms/backoffice/variant';

/**
 * Validation Data Path query generator for Variant.
 * write a JSON-Path filter similar to `?(@.culture == 'en-us' && @.segment == 'mySegment')`
 * where segment are optional.
 * @param value
 * @returns
 */
export function UmbDataPathVariantQuery(
	value: UmbPartialSome<Pick<UmbVariantPropertyValueModel, 'culture' | 'segment'>, 'segment'>,
): string {
	// write a array of strings for each property, where culture must be present and segment is optional
	const filters: Array<string> = [`@.culture == ${value.culture ? `'${value.culture}'` : 'null'}`];
	if (value.segment !== undefined) {
		filters.push(`@.segment == ${value.segment ? `'${value.segment}'` : 'null'}`);
	}
	return `?(${filters.join(' && ')})`;
}
