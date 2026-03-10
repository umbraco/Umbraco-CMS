import type { UmbDocumentVariantOptionModel } from './types.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

type VariantType = UmbDocumentVariantOptionModel;

const variantStatesOrder = {
	[DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES]: 1,
	[DocumentVariantStateModel.PUBLISHED]: 1,
	[DocumentVariantStateModel.DRAFT]: 2,
	[DocumentVariantStateModel.NOT_CREATED]: 3,
	[DocumentVariantStateModel.TRASHED]: 4,
};

const getVariantStateOrderValue = (variant?: UmbDocumentVariantOptionModel['variant']) => {
	const fallbackOrder = 99;

	if (!variant || !variant.state) {
		return fallbackOrder;
	}

	return variantStatesOrder[variant.state] || fallbackOrder;
};

// eslint-disable-next-line jsdoc/require-jsdoc
function compareDefault(a: VariantType, b: VariantType) {
	return (a.language?.isDefault ? -1 : 1) - (b.language?.isDefault ? -1 : 1);
}
// Make sure mandatory variants goes on top, unless they are published, cause then they already goes to the top and then we want to mix them with other published variants.
// eslint-disable-next-line jsdoc/require-jsdoc
function compareMandatory(a: VariantType, b: VariantType) {
	return a.variant?.state === DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES ||
		a.variant?.state === DocumentVariantStateModel.PUBLISHED
		? 0
		: (a.language?.isMandatory ? -1 : 1) - (b.language?.isMandatory ? -1 : 1);
}

// eslint-disable-next-line jsdoc/require-jsdoc
function compareState(a: VariantType, b: VariantType) {
	return getVariantStateOrderValue(a.variant) - getVariantStateOrderValue(b.variant);
}

// eslint-disable-next-line jsdoc/require-jsdoc
function compareName(a: VariantType, b: VariantType) {
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
 * Sorts document variants based on multiple criteria:
 * @param {VariantType} a - First variant to compare
 * @param {VariantType} b - Second variant to compare
 * @returns {number} - Sorting value
 */
export function sortVariants(a: VariantType, b: VariantType) {
	return compareDefault(a, b) || compareMandatory(a, b) || compareState(a, b) || compareName(a, b);
}

export const TimeOptions: Intl.DateTimeFormatOptions = {
	year: 'numeric',
	month: 'long',
	day: 'numeric',
	hour: 'numeric',
	minute: 'numeric',
	second: 'numeric',
};
