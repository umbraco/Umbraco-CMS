import type { UmbMemberVariantOptionModel } from './types.js';

type VariantType = UmbMemberVariantOptionModel;

export const sortVariants = (a: VariantType, b: VariantType) => {
	const compareDefault = (a: VariantType, b: VariantType) =>
		(a.language?.isDefault ? -1 : 1) - (b.language?.isDefault ? -1 : 1);

	// Make sure mandatory variants goes on top.
	const compareMandatory = (a: VariantType, b: VariantType) =>
		(a.language?.isMandatory ? -1 : 1) - (b.language?.isMandatory ? -1 : 1);

	const compareName = (a: VariantType, b: VariantType) => a.variant?.name.localeCompare(b.variant?.name || '') || 99;

	return compareDefault(a, b) || compareMandatory(a, b) || compareName(a, b);
};
