import { UmbPublishableVariantState } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

/**
 * @function isNotPublishedMandatory
 * @param {UmbEntityVariantOptionModel} option - the option to check.
 * @returns {boolean} boolean
 */
export function isNotPublishedMandatory(option: UmbEntityVariantOptionModel): boolean {
	return (
		option.language.isMandatory &&
		option.variant?.state !== UmbPublishableVariantState.PUBLISHED &&
		option.variant?.state !== UmbPublishableVariantState.PUBLISHED_PENDING_CHANGES
	);
}
