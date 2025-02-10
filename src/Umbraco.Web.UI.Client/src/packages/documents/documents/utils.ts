import type { UmbDocumentVariantOptionModel } from './types.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

type VariantType = UmbDocumentVariantOptionModel;

const variantStatesOrder = {
	[DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES]: 1,
	[DocumentVariantStateModel.PUBLISHED]: 1,
	[DocumentVariantStateModel.DRAFT]: 2,
	[DocumentVariantStateModel.NOT_CREATED]: 3,
};

const getVariantStateOrderValue = (variant?: UmbDocumentVariantOptionModel['variant']) => {
	const fallbackOrder = 99;

	if (!variant || !variant.state) {
		return fallbackOrder;
	}

	return variantStatesOrder[variant.state] || fallbackOrder;
};

export const sortVariants = (a: VariantType, b: VariantType) => {
	const compareDefault = (a: VariantType, b: VariantType) =>
		(a.language?.isDefault ? -1 : 1) - (b.language?.isDefault ? -1 : 1);

	// Make sure mandatory variants goes on top, unless they are published, cause then they already goes to the top and then we want to mix them with other published variants.
	const compareMandatory = (a: VariantType, b: VariantType) =>
		a.variant?.state === DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES ||
		a.variant?.state === DocumentVariantStateModel.PUBLISHED
			? 0
			: (a.language?.isMandatory ? -1 : 1) - (b.language?.isMandatory ? -1 : 1);
	const compareState = (a: VariantType, b: VariantType) =>
		getVariantStateOrderValue(a.variant) - getVariantStateOrderValue(b.variant);

	const compareName = (a: VariantType, b: VariantType) => a.variant?.name.localeCompare(b.variant?.name || '') || 99;

	return compareDefault(a, b) || compareMandatory(a, b) || compareState(a, b) || compareName(a, b);
};

export const TimeOptions: Intl.DateTimeFormatOptions = {
	year: 'numeric',
	month: 'long',
	day: 'numeric',
	hour: 'numeric',
	minute: 'numeric',
	second: 'numeric',
};
