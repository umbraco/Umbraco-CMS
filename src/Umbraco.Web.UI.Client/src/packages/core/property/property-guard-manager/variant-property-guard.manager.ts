import type { UmbPropertyGuardRule } from './property-guard.manager.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbGuardManagerBase } from '@umbraco-cms/backoffice/utils';

export interface UmbVariantPropertyGuardRule extends UmbPropertyGuardRule {
	/**
	 * @description - The variant id of the property.
	 * @type {UmbVariantId}
	 * @memberof UmbVariantPropertyGuardRule
	 */
	variantId?: UmbVariantId;

	/**
	 * @description - The variant id of the dataset. This is used to determine if the rule applies to the current dataset.
	 * @type {UmbVariantId}
	 * @memberof UmbVariantPropertyGuardRule
	 */
	datasetVariantId?: UmbVariantId;
}

/**
 *
 * @param {UmbVariantPropertyGuardRule} rule - The rule to check.
 * @param {UmbVariantId} variantId - The property variant id to check.
 * @param {UmbReferenceByUnique} propertyType - The property type to check.
 * @param {UmbVariantId} datasetVariantId - The variant id of the dataset. This is used to determine if the rule applies to the current dataset.
 * @returns {boolean} - Returns true if the rule applies to the given conditions.
 */
function findRule(
	rule: UmbVariantPropertyGuardRule,
	variantId: UmbVariantId,
	propertyType: UmbReferenceByUnique,
	datasetVariantId: UmbVariantId,
) {
	return (
		(rule.variantId === undefined || rule.variantId.culture === variantId.culture) &&
		(rule.propertyType === undefined || rule.propertyType.unique === propertyType.unique) &&
		(rule.datasetVariantId === undefined || rule.datasetVariantId.culture === datasetVariantId.culture)
	);
}

/**
 * UmbVariantPropertyGuardManager is a class that manages the rules for variant properties.
 * @export
 * @class UmbVariantPropertyGuardManager
 * @augments {UmbGuardManagerBase<UmbVariantPropertyGuardRule>}
 */
export class UmbVariantPropertyGuardManager extends UmbGuardManagerBase<UmbVariantPropertyGuardRule> {
	/**
	 * Checks if the variant and propertyType is permitted.
	 * @param {UmbVariantId} propertyVariantId - The variant id to check.
	 * @param {UmbReferenceByUnique} propertyType - The property type to check.
	 * @param {UmbVariantId} datasetVariantId - The dataset variant id to check.
	 * @returns {Observable<boolean>} - Returns an observable that emits true if the variant and propertyType is permitted, false otherwise.
	 * @memberof UmbVariantPropertyGuardManager
	 */
	isPermittedForVariantAndProperty(
		propertyVariantId: UmbVariantId,
		propertyType: UmbReferenceByUnique,
		datasetVariantId: UmbVariantId,
	): Observable<boolean> {
		return this._rules.asObservablePart((rules) =>
			this.#resolvePermission(rules, propertyVariantId, propertyType, datasetVariantId),
		);
	}

	/**
	 * Checks if the variant and propertyType is permitted.
	 * @param {UmbVariantId} propertyVariantId - The variant id to check.
	 * @param {UmbReferenceByUnique} propertyType - The property type to check.
	 * @param {UmbVariantId} datasetVariantId - The dataset variant id to check.
	 * @returns {boolean} - Returns true if the variant and propertyType is permitted, false otherwise.
	 * @memberof UmbVariantPropertyGuardManager
	 */
	getIsPermittedForVariantAndProperty(
		propertyVariantId: UmbVariantId,
		propertyType: UmbReferenceByUnique,
		datasetVariantId: UmbVariantId,
	): boolean {
		return this.#resolvePermission(this._rules.getValue(), propertyVariantId, propertyType, datasetVariantId);
	}

	#resolvePermission(
		rules: UmbVariantPropertyGuardRule[],
		propertyVariantId: UmbVariantId,
		propertyType: UmbReferenceByUnique,
		datasetVariantId: UmbVariantId,
	) {
		if (
			rules
				.filter((x) => x.permitted === false)
				.some((rule) => findRule(rule, propertyVariantId, propertyType, datasetVariantId))
		) {
			return false;
		}
		if (
			rules
				.filter((x) => x.permitted === true)
				.some((rule) => findRule(rule, propertyVariantId, propertyType, datasetVariantId))
		) {
			return true;
		}
		return this._fallback;
	}
}
