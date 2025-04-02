import { type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbStateManager, type UmbStateEntry } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyState extends UmbStateEntry {
	propertyType?: UmbReferenceByUnique;
}

function ComparePropertyRefWithStates(states: UmbPropertyState[], propertyType: UmbReferenceByUnique) {
	// any specific states for the propertyType?
	const propertyState = states.find((s) => s.propertyType?.unique === propertyType.unique);
	if (propertyState) {
		return propertyState.state;
	}

	// any state without variant:
	const nonVariantState = states.find((s) => s.propertyType === undefined);
	if (nonVariantState) {
		return nonVariantState.state;
	}

	return false;
}

export class UmbPropertyStateManager extends UmbStateManager<UmbPropertyState> {
	//
	isOnForProperty(propertyType: UmbReferenceByUnique): Observable<boolean> {
		return this._states.asObservablePart((states) => ComparePropertyRefWithStates(states, propertyType));
	}

	getIsOnForVariant(propertyType: UmbReferenceByUnique): boolean {
		return ComparePropertyRefWithStates(this._states.getValue(), propertyType);
	}
}
