import type { UmbReferenceByVariantId } from '@umbraco-cms/backoffice/variant';

/**
 *
 * @param {unknown} data  The data to check if it is a ReferencedByVariantId
 * @returns {boolean} True if the data is a ReferencedByVariantId
 */
export function isReferenceByVariantId(data: unknown): data is UmbReferenceByVariantId {
	return (data as UmbReferenceByVariantId).variantId !== undefined;
}
