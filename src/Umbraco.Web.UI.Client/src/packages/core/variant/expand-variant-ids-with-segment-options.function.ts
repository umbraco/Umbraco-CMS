import { UmbVariantId } from './variant-id.class.js';
import type { UmbEntityVariantOptionModel } from './types.js';

/**
 * Expands a variant id selection so that every selected culture (including the invariant
 * culture) is represented by all of its available segment variants. A variant id that already
 * targets a specific segment is kept as-is.
 * @param {Array<UmbVariantId>} variantIds - The selected variant ids to expand.
 * @param {Array<UmbEntityVariantOptionModel>} variantOptions - The available variant options to expand against.
 * @returns {Array<UmbVariantId>} The expanded variant ids.
 */
export function umbExpandVariantIdsWithSegmentOptions(
	variantIds: Array<UmbVariantId>,
	variantOptions: Array<UmbEntityVariantOptionModel>,
): Array<UmbVariantId> {
	return variantIds.flatMap((variantId) => {
		if (variantId.segment !== null) return [variantId];
		const segmentOptions = variantOptions
			.filter((option) => option.culture === variantId.culture)
			.map((option) => UmbVariantId.Create(option));
		return segmentOptions.length > 0 ? segmentOptions : [variantId];
	});
}
