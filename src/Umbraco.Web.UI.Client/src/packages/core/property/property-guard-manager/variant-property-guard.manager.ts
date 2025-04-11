import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyGuardRule } from './property-guard.manager.js';
import { UmbGuardManagerBase } from '@umbraco-cms/backoffice/utils';

export interface UmbVariantPropertyGuardRule extends UmbPropertyGuardRule {
	variantId?: UmbVariantId;
}

function findRule(rule: UmbVariantPropertyGuardRule, variantId: UmbVariantId, propertyType: UmbReferenceByUnique) {
	return (
		(rule.variantId?.compare(variantId) && rule.propertyType?.unique === propertyType.unique) ||
		(rule.variantId === undefined && rule.propertyType?.unique === propertyType.unique) ||
		(rule.variantId?.compare(variantId) && rule.propertyType === undefined) ||
		(rule.variantId === undefined && rule.propertyType === undefined)
	);
}

/**
 * UmbVariantPropertyGuardManager is a class that manages the rules for variant properties.
 * @export
 * @class UmbVariantPropertyGuardManager
 * @extends {UmbGuardManagerBase<UmbVariantPropertyGuardRule>}
 */
export class UmbVariantPropertyGuardManager extends UmbGuardManagerBase<UmbVariantPropertyGuardRule> {
	/**
	 * Checks if the variant and propertyType is permitted.
	 * @param {UmbVariantId} variantId - The variant id to check.
	 * @param {UmbReferenceByUnique} propertyType - The property type to check.
	 * @return {Observable<boolean>} - Returns an observable that emits true if the variant and propertyType is permitted, false otherwise.
	 * @memberof UmbVariantPropertyGuardManager
	 */
	isPermittedForVariantAndProperty(variantId: UmbVariantId, propertyType: UmbReferenceByUnique): Observable<boolean> {
		return this._rules.asObservablePart((rules) => this.#resolvePermission(rules, variantId, propertyType));
	}

	/**
	 * Checks if the variant and propertyType is permitted.
	 * @param {UmbVariantId} variantId - The variant id to check.
	 * @param {UmbReferenceByUnique} propertyType - The property type to check.
	 * @return {boolean} - Returns true if the variant and propertyType is permitted, false otherwise.
	 * @memberof UmbVariantPropertyGuardManager
	 */
	getIsPermittedForVariantAndProperty(variantId: UmbVariantId, propertyType: UmbReferenceByUnique): boolean {
		return this.#resolvePermission(this._rules.getValue(), variantId, propertyType);
	}

	#resolvePermission(
		rules: UmbVariantPropertyGuardRule[],
		variantId: UmbVariantId,
		propertyType: UmbReferenceByUnique,
	) {
		if (rules.filter((x) => x.permitted === false).some((rule) => findRule(rule, variantId, propertyType))) {
			return false;
		}
		if (rules.filter((x) => x.permitted === true).some((rule) => findRule(rule, variantId, propertyType))) {
			return true;
		}
		return this._fallback;
	}
}
