import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyGuardRule } from './property-guard.manager.js';
import { UmbGuardManagerBase } from '@umbraco-cms/backoffice/utils';

export interface UmbVariantPropertyGuardRule extends UmbPropertyGuardRule {
	variantId?: UmbVariantId;
}

function compareVariantAndPropertyWithStates(
	rules: UmbVariantPropertyGuardRule[],
	variantId: UmbVariantId,
	propertyType: UmbReferenceByUnique,
) {
	// any specific states for the variant and propertyType?
	const variantAndPropertyState = rules.find(
		(s) => s.variantId?.compare(variantId) && s.propertyType?.unique === propertyType.unique,
	);
	if (variantAndPropertyState) {
		return variantAndPropertyState.permitted;
	}

	// any specific states for the propertyType?
	const propertyState = rules.find((s) => s.variantId === undefined && s.propertyType?.unique === propertyType.unique);
	if (propertyState) {
		return propertyState.permitted;
	}

	// any specific states for the variant?
	const variantState = rules.find((s) => s.variantId?.compare(variantId) && s.propertyType === undefined);
	if (variantState) {
		return variantState.permitted;
	}

	// any state without variant:
	const nonVariantState = rules.find((s) => s.variantId === undefined && s.propertyType === undefined);
	if (nonVariantState) {
		return nonVariantState.permitted;
	}

	return false;
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
		return this._rules.asObservablePart((rules) => compareVariantAndPropertyWithStates(rules, variantId, propertyType));
	}

	/**
	 * Checks if the variant and propertyType is permitted.
	 * @param {UmbVariantId} variantId - The variant id to check.
	 * @param {UmbReferenceByUnique} propertyType - The property type to check.
	 * @return {boolean} - Returns true if the variant and propertyType is permitted, false otherwise.
	 * @memberof UmbVariantPropertyGuardManager
	 */
	getIsPermittedForVariantAndProperty(variantId: UmbVariantId, propertyType: UmbReferenceByUnique): boolean {
		return compareVariantAndPropertyWithStates(this._rules.getValue(), variantId, propertyType);
	}
}
