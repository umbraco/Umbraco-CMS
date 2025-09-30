import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../types.js';

/**
 * @function isNotPublishedMandatory
 * @param {UmbDocumentVariantOptionModel} option - the option to check.
 * @returns {boolean} boolean
 */
export function isNotPublishedMandatory(option: UmbDocumentVariantOptionModel): boolean {
	return (
		option.language.isMandatory &&
		option.variant?.state !== UmbDocumentVariantState.PUBLISHED &&
		option.variant?.state !== UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES
	);
}
