import { UmbElementVariantState, type UmbElementVariantOptionModel } from '../types.js';

/**
 * @function isNotPublishedMandatory
 * @param {UmbElementVariantOptionModel} option - the option to check.
 * @returns {boolean} boolean
 */
export function isNotPublishedMandatory(option: UmbElementVariantOptionModel): boolean {
	return (
		option.language.isMandatory &&
		option.variant?.state !== UmbElementVariantState.PUBLISHED &&
		option.variant?.state !== UmbElementVariantState.PUBLISHED_PENDING_CHANGES
	);
}
