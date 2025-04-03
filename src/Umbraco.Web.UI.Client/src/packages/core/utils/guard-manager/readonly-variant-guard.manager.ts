import { UmbReadonlyGuardManager } from './readonly-guard.manager.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbGuardRule } from './guard.manager.base.js';

export interface UmbVariantGuardRule extends UmbGuardRule {
	variantId?: UmbVariantId;
}

function CompareStateAndVariantId(rules: Array<UmbVariantGuardRule>, variantId: UmbVariantId): boolean {
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

export class UmbReadonlyVariantGuardManager extends UmbReadonlyGuardManager<UmbVariantGuardRule> {
	//
	permittedForVariant(variantId: UmbVariantId): Observable<boolean> {
		return this._rules.asObservablePart((states) => {
			return CompareStateAndVariantId(states, variantId);
		});
	}

	permittedForVariantObservable(variantId: Observable<UmbVariantId | undefined>): Observable<boolean> {
		return mergeObservables([this.rules, variantId], ([states, variantId]) => {
			if (!variantId) {
				// Or should we know about the fallback state here? [NL]
				return false;
			}
			return CompareStateAndVariantId(states, variantId);
		});
	}

	getPermittedForVariant(variantId: UmbVariantId): boolean {
		return CompareStateAndVariantId(this.getRules(), variantId);
	}
}
