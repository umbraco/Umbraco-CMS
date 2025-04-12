import { UmbReadOnlyGuardManager } from './readonly-guard.manager.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbGuardRule } from './guard.manager.base.js';

export interface UmbVariantGuardRule extends UmbGuardRule {
	variantId?: UmbVariantId;
}

function findRule(rule: UmbVariantGuardRule, variantId: UmbVariantId) {
	return rule.variantId?.compare(variantId) || rule.variantId === undefined;
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
	 * @return {Observable<boolean>} - Observable that emits true if the variantId is permitted to read, false otherwise
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	isPermittedForVariant(variantId: UmbVariantId): Observable<boolean> {
		return this._rules.asObservablePart((states) => {
			return this.#resolvePermission(states, variantId);
		});
	}

	/**
	 * @param {Observable<UmbVariantId | undefined>} variantId
	 * @return {Observable<boolean>} - Observable that emits true if the variantId is permitted to read, false otherwise
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	isPermittedForObservableVariant(variantId: Observable<UmbVariantId | undefined>): Observable<boolean> {
		return mergeObservables([this.rules, variantId], ([states, variantId]) => {
			if (!variantId) {
				// Or should we know about the fallback state here? [NL]
				return false;
			}
			return this.#resolvePermission(states, variantId);
		});
	}

	/**
	 * Check if the given variantId is permitted to read
	 * @param {UmbVariantId} variantId
	 * @return {boolean} - true if the variantId is permitted to read, false otherwise
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	getIsPermittedForVariant(variantId: UmbVariantId): boolean {
		return this.#resolvePermission(this.getRules(), variantId);
	}

	#resolvePermission(rules: UmbVariantGuardRule[], variantId: UmbVariantId) {
		if (rules.filter((x) => x.permitted === false).some((rule) => findRule(rule, variantId))) {
			return false;
		}
		if (rules.filter((x) => x.permitted === true).some((rule) => findRule(rule, variantId))) {
			return true;
		}
		return this._fallback;
	}
}
