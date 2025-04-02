import type { UmbStateEntry } from './state.manager.js';
import { UmbReadOnlyStateManager } from './read-only-state.manager.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbVariantState extends UmbStateEntry {
	variantId?: UmbVariantId;
}

function CompareStateAndVariantId(states: Array<UmbVariantState>, variantId: UmbVariantId): boolean {
	// any specific states for the variant?
	const variantState = states.find((s) => s.variantId?.compare(variantId));
	if (variantState) {
		return variantState.state;
	}

	// any state without variant:
	const nonVariantState = states.find((s) => s.variantId === undefined);
	if (nonVariantState) {
		return nonVariantState.state;
	}

	return false;
}

export class UmbReadOnlyVariantStateManager extends UmbReadOnlyStateManager<UmbVariantState> {
	//
	isOnForVariant(variantId: UmbVariantId): Observable<boolean> {
		return this._states.asObservablePart((states) => {
			return CompareStateAndVariantId(states, variantId);
		});
	}

	isOnForVariantObservable(variantId: Observable<UmbVariantId | undefined>): Observable<boolean> {
		return mergeObservables([this._statesObservable, variantId], ([states, variantId]) => {
			if (!variantId) {
				// Or should we know about the fallback state here? [NL]
				return false;
			}
			return CompareStateAndVariantId(states, variantId);
		});
	}

	getIsOnForVariant(variantId: UmbVariantId): boolean {
		return CompareStateAndVariantId(this._states.getValue(), variantId);
	}
}
