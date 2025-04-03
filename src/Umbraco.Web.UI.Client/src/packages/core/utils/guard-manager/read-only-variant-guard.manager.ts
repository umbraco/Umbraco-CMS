import { UmbReadOnlyGuardManager } from './read-only-guard.manager.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbGuardRuleEntry } from './guard.manager.base.js';

export interface UmbVariantState extends UmbGuardRuleEntry {
	variantId?: UmbVariantId;
}

function CompareStateAndVariantId(rules: Array<UmbVariantState>, variantId: UmbVariantId): boolean {
	// any specific states for the variant?
	const variantState = rules.find((s) => s.variantId?.compare(variantId));
	if (variantState) {
		return variantState.permitted;
	}

	// any state without variant:
	const nonVariantState = rules.find((s) => s.variantId === undefined);
	if (nonVariantState) {
		return nonVariantState.permitted;
	}

	return false;
}

export class UmbReadOnlyVariantGuardManager extends UmbReadOnlyGuardManager<UmbVariantState> {
	//
	isOnForVariant(variantId: UmbVariantId): Observable<boolean> {
		return this._rules.asObservablePart((states) => {
			return CompareStateAndVariantId(states, variantId);
		});
	}

	isOnForVariantObservable(variantId: Observable<UmbVariantId | undefined>): Observable<boolean> {
		return mergeObservables([this.rules, variantId], ([states, variantId]) => {
			if (!variantId) {
				// Or should we know about the fallback state here? [NL]
				return false;
			}
			return CompareStateAndVariantId(states, variantId);
		});
	}

	getIsOnForVariant(variantId: UmbVariantId): boolean {
		return CompareStateAndVariantId(this.getRules(), variantId);
	}
}
