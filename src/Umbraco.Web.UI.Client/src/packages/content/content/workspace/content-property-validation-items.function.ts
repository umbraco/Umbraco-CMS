import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbContentPropertyValidationItem {
	alias: string;
	variantId: UmbVariantId;
}

/**
 * Flattens the current content type structure's properties against the current variant options into
 * one entry per (property, applicable variant) combination — the granularity content property
 * validation messages are actually scoped at (`$.values[?(@.alias == 'x' && @.culture == ... && @.segment == ...)]`).
 * @param {Array} properties - The current content type properties (from `UmbContentTypeStructureManager.contentTypeProperties`).
 * @param {Array} variantOptions - The current variant options (from the content workspace's `variantOptions`).
 * @returns {Array<UmbContentPropertyValidationItem>} One entry per applicable (alias, variant) combination.
 */
export function umbGetContentPropertyValidationItems(
	properties: Array<{ alias: string; variesByCulture?: boolean; variesBySegment?: boolean }>,
	variantOptions: Array<{ culture: string | null; segment: string | null }>,
): Array<UmbContentPropertyValidationItem> {
	const seen = new Set<string>();
	const result: Array<UmbContentPropertyValidationItem> = [];
	for (const property of properties) {
		for (const option of variantOptions) {
			// A culture-invariant variant option can not serve a culture-varying property: [NL]
			if (property.variesByCulture && option.culture === null) continue;

			const variantId = new UmbVariantId(option.culture, option.segment).toVariant(
				property.variesByCulture,
				property.variesBySegment,
			);
			const key = property.alias + '_' + variantId.toString();
			if (seen.has(key)) continue;
			seen.add(key);
			result.push({ alias: property.alias, variantId });
		}
	}
	return result;
}
