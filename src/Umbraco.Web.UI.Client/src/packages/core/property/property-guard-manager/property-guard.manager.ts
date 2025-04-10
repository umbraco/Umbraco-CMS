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

export class UmbPropertyGuardManager extends UmbGuardManagerBase<UmbPropertyGuardRule> {
	//
	isPermittedForProperty(propertyType: UmbReferenceByUnique): Observable<boolean> {
		return this._rules.asObservablePart((rules) => comparePropertyRefWithStates(rules, propertyType));
	}

	getIsPermittedForProperty(propertyType: UmbReferenceByUnique): boolean {
		return comparePropertyRefWithStates(this.getRules(), propertyType);
	}
}
