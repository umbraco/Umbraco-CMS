import { type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbGuardManagerBase, type UmbGuardRule } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyGuardRule extends UmbGuardRule {
	propertyType?: UmbReferenceByUnique;
}

function comparePropertyRefWithStates(rules: UmbPropertyGuardRule[], propertyType: UmbReferenceByUnique) {
	// any specific states for the propertyType?
	const propertyState = rules.find((s) => s.propertyType?.unique === propertyType.unique);
	if (propertyState) {
		return propertyState.permitted;
	}

	// any state without variant:
	const nonVariantState = rules.find((s) => s.propertyType === undefined);
	if (nonVariantState) {
		return nonVariantState.permitted;
	}

	return false;
}

/**
 * @description - A Guard to manage property rules.
 * @export
 * @class UmbPropertyGuardManager
 * @extends {UmbGuardManagerBase<UmbPropertyGuardRule>}
 */
export class UmbPropertyGuardManager extends UmbGuardManagerBase<UmbPropertyGuardRule> {
	/**
	 * Checks if the property is permitted for the given property type
	 * @param {UmbReferenceByUnique} propertyType
	 * @return {Observable<boolean>} - Observable that emits true if the property is permitted
	 * @memberof UmbPropertyGuardManager
	 */
	isPermittedForProperty(propertyType: UmbReferenceByUnique): Observable<boolean> {
		return this._rules.asObservablePart((rules) => comparePropertyRefWithStates(rules, propertyType));
	}

	/**
	 * Checks if the property is permitted for the given property type
	 * @param {UmbReferenceByUnique} propertyType
	 * @return {boolean} - Returns true if the property is permitted
	 * @memberof UmbPropertyGuardManager
	 */
	getIsPermittedForProperty(propertyType: UmbReferenceByUnique): boolean {
		return comparePropertyRefWithStates(this.getRules(), propertyType);
	}
}
