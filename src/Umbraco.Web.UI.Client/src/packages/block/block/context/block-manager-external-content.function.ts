import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbBlockManagerExternalContentVariantModel {
	culture: string | null;
	state: string | null;
	name: string | null;
}

/**
 * Finds the external content variant matching the given variant (by culture), falling back to the
 * first variant when there is no match, or no variant to match against.
 * @param {Array<UmbBlockManagerExternalContentVariantModel>} [variants] - the external content's variants.
 * @param {UmbVariantId} [variantId] - the variant to match.
 * @returns {UmbBlockManagerExternalContentVariantModel | undefined} the matching variant, or the first variant as a fallback.
 */
export function findExternalContentVariant(
	variants: Array<UmbBlockManagerExternalContentVariantModel> | undefined,
	variantId: UmbVariantId | undefined,
): UmbBlockManagerExternalContentVariantModel | undefined {
	if (!variants?.length) return undefined;
	if (!variantId) return variants[0];
	return variants.find((x) => x.culture === variantId.culture) ?? variants[0];
}
