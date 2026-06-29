import type { UmbEntityVariantOptionModel } from './types.js';
import { DocumentVariantStateModel as UmbPublishableVariantState } from '@umbraco-cms/backoffice/external/backend-api';

const variantStatesOrder: Record<string, number> = {
	[UmbPublishableVariantState.PUBLISHED_PENDING_CHANGES]: 1,
	[UmbPublishableVariantState.PUBLISHED]: 1,
	[UmbPublishableVariantState.DRAFT]: 2,
	[UmbPublishableVariantState.NOT_CREATED]: 3,
	[UmbPublishableVariantState.TRASHED]: 4,
};

const getVariantStateOrderValue = (variant?: UmbEntityVariantOptionModel['variant']) => {
	const fallbackOrder = 99;

	if (!variant || !variant.state) {
		return fallbackOrder;
	}

	return variantStatesOrder[variant.state] || fallbackOrder;
};

/**
 * @param {UmbEntityVariantOptionModel} a - First variant to compare
 * @param {UmbEntityVariantOptionModel} b - Second variant to compare
 * @returns {number} - Sorting value
 */
function compareDefault(a: UmbEntityVariantOptionModel, b: UmbEntityVariantOptionModel): number {
	return (a.language?.isDefault ? -1 : 1) - (b.language?.isDefault ? -1 : 1);
}

/**
 * Mandatory variants sort to the top, unless they are published — published variants already sort first and should mix with other published variants.
 * @param {UmbEntityVariantOptionModel} a - First variant to compare
 * @param {UmbEntityVariantOptionModel} b - Second variant to compare
 * @returns {number} - Sorting value
 */
function compareMandatory(a: UmbEntityVariantOptionModel, b: UmbEntityVariantOptionModel): number {
	return a.variant?.state === UmbPublishableVariantState.PUBLISHED_PENDING_CHANGES ||
		a.variant?.state === UmbPublishableVariantState.PUBLISHED
		? 0
		: (a.language?.isMandatory ? -1 : 1) - (b.language?.isMandatory ? -1 : 1);
}

/**
 * @param {UmbEntityVariantOptionModel} a - First variant to compare
 * @param {UmbEntityVariantOptionModel} b - Second variant to compare
 * @returns {number} - Sorting value
 */
function compareState(a: UmbEntityVariantOptionModel, b: UmbEntityVariantOptionModel): number {
	return getVariantStateOrderValue(a.variant) - getVariantStateOrderValue(b.variant);
}

/**
 * @param {UmbEntityVariantOptionModel} a - First variant to compare
 * @param {UmbEntityVariantOptionModel} b - Second variant to compare
 * @returns {number} - Sorting value
 */
function compareName(a: UmbEntityVariantOptionModel, b: UmbEntityVariantOptionModel): number {
	const nameA = a.language?.name;
	const nameB = b.language?.name;

	// If both names are missing, consider them equal.
	if (!nameA && !nameB) return 0;
	// If only one name is missing, sort the defined name first.
	if (!nameA) return 1;
	if (!nameB) return -1;

	return nameA.localeCompare(nameB);
}
/**
 * Sorts entity variants based on multiple criteria:
 * @param {UmbEntityVariantOptionModel} a - First variant to compare
 * @param {UmbEntityVariantOptionModel} b - Second variant to compare
 * @returns {number} - Sorting value
 */
export function sortVariants(a: UmbEntityVariantOptionModel, b: UmbEntityVariantOptionModel): number {
	return compareDefault(a, b) || compareMandatory(a, b) || compareState(a, b) || compareName(a, b);
}
