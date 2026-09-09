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

/**
 * @function isSelectableForPublishing
 * @param {UmbEntityVariantOptionModel} option - the option to check.
 * @returns {boolean} boolean
 * @description A variant can only be published if it holds data, so a variant that has not been created is not
 * selectable — unless its language is mandatory and not yet published, in which case it is listed as a requirement.
 * A variant whose state is unknown (e.g. from a bulk selection) has to be considered selectable.
 */
export function isSelectableForPublishing(option: UmbEntityVariantOptionModel): boolean {
	return (
		isNotPublishedMandatory(option) ||
		(!!option.variant && option.variant.state !== UmbPublishableVariantState.NOT_CREATED)
	);
}
