import type { UmbPropertyValueDataWithVariant } from '@umbraco-cms/backoffice/property';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * Resolves a single value per property for a given variant.
 * @param {Array<{ alias: string; variantId: UmbVariantId }>} propertyVariantIds - The property types paired with the
 * variant id to extract values for.
 * @param {Array<UmbPropertyValueDataWithVariant> | undefined} values - The full value set to pick from.
 * @returns {Array<UmbPropertyValueDataWithVariant>} One value per property whose alias and variant matches, in property order.
 */
export function umbExtractVariantValues<ValueType extends UmbPropertyValueDataWithVariant>(
	propertyVariantIds: Array<{ alias: string; variantId: UmbVariantId }>,
	values: Array<ValueType> | undefined,
): Array<ValueType> {
	const result: Array<ValueType> = [];
	if (values) {
		for (const property of propertyVariantIds) {
			const found = values.find((value) => property.alias === value.alias && property.variantId.compare(value));
			if (found) {
				result.push(found);
			}
		}
	}
	return result;
}
