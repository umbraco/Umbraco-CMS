import type { UmbLanguageDetailModel } from './types.js';

const compareDefault = (a: UmbLanguageDetailModel, b: UmbLanguageDetailModel) =>
	(a.isDefault ? -1 : 1) - (b.isDefault ? -1 : 1);

const compareMandatory = (a: UmbLanguageDetailModel, b: UmbLanguageDetailModel) =>
	(a.isMandatory ? -1 : 1) - (b.isMandatory ? -1 : 1);

const compareName = (a: UmbLanguageDetailModel, b: UmbLanguageDetailModel) => {
	const nameA = a.name;
	const nameB = b.name;

	// If both names are missing, consider them equal.
	if (!nameA && !nameB) return 0;

	// If only one name is missing, sort the defined name first.
	if (!nameA) return 1;
	if (!nameB) return -1;

	return nameA.localeCompare(nameB);
};

/**
 * Sorts languages by: default first, then mandatory, then alphabetically by name.
 * @param {UmbLanguageDetailModel} a - First language to compare
 * @param {UmbLanguageDetailModel} b - Second language to compare
 * @returns {number} - Sorting value
 */
export const sortLanguages = (a: UmbLanguageDetailModel, b: UmbLanguageDetailModel) =>
	compareDefault(a, b) || compareMandatory(a, b) || compareName(a, b);
