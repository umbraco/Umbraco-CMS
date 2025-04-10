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
/**
 * Read only guard manager for variant rules.
 * @export
 * @class UmbReadOnlyVariantGuardManager
 * @extends {UmbReadOnlyGuardManager<UmbVariantGuardRule>}
 */
export class UmbReadOnlyVariantGuardManager extends UmbReadOnlyGuardManager<UmbVariantGuardRule> {
	/**
	 * Observe if the given variantId is permitted to read
	 * @param {UmbVariantId} variantId
	 * @return {Observable<boolean>} - true if the variantId is permitted to read, false otherwise
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	isPermittedForVariant(variantId: UmbVariantId): Observable<boolean> {
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

	/**
	 * Check if the given variantId is permitted to read
	 * @param {UmbVariantId} variantId
	 * @return {boolean} - true if the variantId is permitted to read, false otherwise
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	getIsPermittedForVariant(variantId: UmbVariantId): boolean {
		return compareStateAndVariantId(this.getRules(), variantId);
	}
}
