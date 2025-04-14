import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbGuardManagerBase, type UmbGuardRule } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyGuardRule extends UmbGuardRule {
	propertyType?: UmbReferenceByUnique;
}

/**
 *
 * @param rule
 * @param propertyType
 */
function findRule(rule: UmbPropertyGuardRule, propertyType: UmbReferenceByUnique) {
	return rule.propertyType?.unique === propertyType.unique || rule.propertyType === undefined;
}

/**
 * @description - A Guard to manage property rules.
 * @export
 * @class UmbPropertyGuardManager
 * @augments {UmbGuardManagerBase<UmbPropertyGuardRule>}
 */
export class UmbPropertyGuardManager extends UmbGuardManagerBase<UmbPropertyGuardRule> {
	/**
	 * Checks if the property is permitted for the given property type
	 * @param {UmbReferenceByUnique} propertyType
	 * @returns {Observable<boolean>} - Observable that emits true if the property is permitted
	 * @memberof UmbPropertyGuardManager
	 */
	isPermittedForProperty(propertyType: UmbReferenceByUnique): Observable<boolean> {
		return this._rules.asObservablePart((rules) => this.#resolvePermission(rules, propertyType));
	}

	/**
	 * Checks if the property is permitted for the given property type
	 * @param {UmbReferenceByUnique} propertyType
	 * @returns {boolean} - Returns true if the property is permitted
	 * @memberof UmbPropertyGuardManager
	 */
	getIsPermittedForProperty(propertyType: UmbReferenceByUnique): boolean {
		return this.#resolvePermission(this.getRules(), propertyType);
	}

	#resolvePermission(rules: UmbPropertyGuardRule[], propertyType: UmbReferenceByUnique) {
		if (rules.filter((x) => x.permitted === false).some((rule) => findRule(rule, propertyType))) {
			return false;
		}
		if (rules.filter((x) => x.permitted === true).some((rule) => findRule(rule, propertyType))) {
			return true;
		}
		return this._fallback;
	}
}
