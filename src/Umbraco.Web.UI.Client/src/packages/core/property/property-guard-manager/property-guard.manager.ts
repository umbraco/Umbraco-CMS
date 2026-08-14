import { mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbGuardManagerBase, type UmbGuardRule } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyGuardRule extends UmbGuardRule {
	propertyType?: UmbReferenceByUnique;
}

/**
 *
 * @param {UmbPropertyGuardRule} rule - The rule to check.
 * @param {UmbReferenceByUnique} propertyType - The property type to check.
 * @returns {boolean} - Returns true if the rule applies to the given property type.
 */
function findRule(rule: UmbPropertyGuardRule, propertyType: UmbReferenceByUnique) {
	return rule.propertyType?.unique === propertyType.unique || rule.propertyType === undefined;
}

/**
 * @description - A Guard to manage property rules.
 * @class UmbPropertyGuardManager
 * @augments {UmbGuardManagerBase<UmbPropertyGuardRule>}
 */
export class UmbPropertyGuardManager extends UmbGuardManagerBase<UmbPropertyGuardRule> {
	/**
	 * Checks if the property is permitted for the given property type
	 * @param {UmbReferenceByUnique} propertyType - The property type to check.
	 * @returns {Observable<boolean>} - Observable that emits true if the property is permitted
	 * @memberof UmbPropertyGuardManager
	 */
	isPermittedForProperty(propertyType: UmbReferenceByUnique): Observable<boolean> {
		return mergeObservables([this.rules, this._fallback], ([states, fallback]) => {
			return this.#resolvePermission(states, propertyType) ?? fallback;
		});
	}

	/**
	 * Checks if the property is permitted for the given property type
	 * @param {UmbReferenceByUnique} propertyType - The property type to check.
	 * @returns {boolean} - Returns true if the property is permitted
	 * @memberof UmbPropertyGuardManager
	 */
	getIsPermittedForProperty(propertyType: UmbReferenceByUnique): boolean {
		return this.#resolvePermission(this.getRules(), propertyType) ?? this._getFallback();
	}

	#resolvePermission(rules: UmbPropertyGuardRule[], propertyType: UmbReferenceByUnique): boolean | undefined {
		if (rules.filter((x) => x.permitted === false).some((rule) => findRule(rule, propertyType))) {
			return false;
		}
		if (rules.filter((x) => x.permitted === true).some((rule) => findRule(rule, propertyType))) {
			return true;
		}
		return undefined;
	}
}
