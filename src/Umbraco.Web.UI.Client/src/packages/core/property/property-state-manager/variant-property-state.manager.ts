import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { type Observable } from '@umbraco-cms/backoffice/observable-api';
import { UmbStateManager } from '@umbraco-cms/backoffice/utils';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyState } from './property-state.manager';

export interface UmbVariantPropertyState extends UmbPropertyState {
	variantId?: UmbVariantId;
}

function CompareVariantAndPropertyWithStates(
	states: UmbVariantPropertyState[],
	variantId: UmbVariantId,
	propertyType: UmbReferenceByUnique,
) {
	// any specific states for the variant and propertyType?
	const variantEndPropertyState = states.find(
		(s) => s.variantId?.compare(variantId) && s.propertyType?.unique === propertyType.unique,
	);
	if (variantEndPropertyState) {
		return variantEndPropertyState.state;
	}

	// any specific states for the propertyType?
	const propertyState = states.find((s) => s.variantId === undefined && s.propertyType?.unique === propertyType.unique);
	if (propertyState) {
		return propertyState.state;
	}

	// any specific states for the variant?
	const variantState = states.find((s) => s.variantId?.compare(variantId) && s.propertyType === undefined);
	if (variantState) {
		return variantState.state;
	}

	// any state without variant:
	const nonVariantState = states.find((s) => s.variantId === undefined && s.propertyType === undefined);
	if (nonVariantState) {
		return nonVariantState.state;
	}

	return false;
}

export class UmbReadOnlyVariantStateManager extends UmbStateManager<UmbVariantPropertyState> {
	//
	isOnForVariantAndProperty(variantId: UmbVariantId, propertyType: UmbReferenceByUnique): Observable<boolean> {
		return this._states.asObservablePart((states) =>
			CompareVariantAndPropertyWithStates(states, variantId, propertyType),
		);
	}

	/*
	isOnForVariantObservableAndProperty(
		variantId: Observable<UmbVariantId | undefined>,
		propertyType: UmbReferenceByUnique,
	): Observable<boolean> {
		return mergeObservables([this._statesObservable, variantId], ([states, variantId]) => {
			if (!variantId) {
				// Or should we know about the fallback state here? [NL]
				return false;
			}
			return CompareVariantAndPropertyWithStates(states, variantId, propertyType);
		});
	}
	*/

	getIsOnForVariant(variantId: UmbVariantId, propertyType: UmbReferenceByUnique): boolean {
		return CompareVariantAndPropertyWithStates(this._states.getValue(), variantId, propertyType);
	}
}
