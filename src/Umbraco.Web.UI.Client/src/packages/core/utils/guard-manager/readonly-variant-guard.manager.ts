import { UmbReadOnlyGuardManager } from './readonly-guard.manager.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbGuardRule } from './guard.manager.base.js';

export interface UmbVariantGuardRule extends UmbGuardRule {
	variantId?: UmbVariantId;
}

function compareStateAndVariantId(rules: Array<UmbVariantGuardRule>, variantId: UmbVariantId): boolean {
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

// TODO: Check the need for this one.
export class UmbReadOnlyVariantGuardManager extends UmbReadOnlyGuardManager<UmbVariantGuardRule> {
	//
	permittedForVariant(variantId: UmbVariantId): Observable<boolean> {
		return this._rules.asObservablePart((states) => {
			return compareStateAndVariantId(states, variantId);
		});
	}

	permittedForVariantObservable(variantId: Observable<UmbVariantId | undefined>): Observable<boolean> {
		return mergeObservables([this.rules, variantId], ([states, variantId]) => {
			if (!variantId) {
				// Or should we know about the fallback state here? [NL]
				return false;
			}
			return compareStateAndVariantId(states, variantId);
		});
	}

	getPermittedForVariant(variantId: UmbVariantId): boolean {
		return compareStateAndVariantId(this.getRules(), variantId);
	}
}
