import { UmbReadOnlyGuardManager } from './readonly-guard.manager.js';
import type { UmbGuardRule } from './guard.manager.base.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbVariantGuardRule extends UmbGuardRule {
	variantId?: UmbVariantId;
}

/**
 *
 * @param rule
 * @param variantId
 */
function findRule(rule: UmbVariantGuardRule, variantId: UmbVariantId) {
	return rule.variantId?.compare(variantId) || rule.variantId === undefined;
}

/**
 * Read only guard manager for variant rules.
 * @export
 * @class UmbReadOnlyVariantGuardManager
 * @augments {UmbReadOnlyGuardManager<UmbVariantGuardRule>}
 */
export class UmbReadOnlyVariantGuardManager extends UmbReadOnlyGuardManager<UmbVariantGuardRule> {
	/**
	 * Observe if the given variantId is permitted to read
	 * @param {UmbVariantId} variantId
	 * @returns {Observable<boolean>} - Observable that emits true if the variantId is permitted to read, false otherwise
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	isPermittedForVariant(variantId: UmbVariantId): Observable<boolean> {
		return mergeObservables(
			[
				this._rules.asObservablePart((rules) => {
					return this.#resolvePermission(rules, variantId);
				}),
				this._fallback,
			],
			([permitted, fallback]) => permitted ?? fallback,
		);
	}

	/**
	 * @param {Observable<UmbVariantId | undefined>} variantId
	 * @returns {Observable<boolean | undefined>} - Observable that emits true if the variantId is permitted to read, false otherwise
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	isPermittedForObservableVariant(variantId: Observable<UmbVariantId | undefined>): Observable<boolean | undefined> {
		return mergeObservables([this.rules, variantId, this._fallback], ([states, variantId, fallback]) => {
			if (!variantId) {
				return undefined;
			}
			return this.#resolvePermission(states, variantId) ?? fallback;
		});
	}

	/**
	 * Observe the permission for multiple given variantIds
	 * @param {Observable<UmbVariantId[]>} variantIds - Observable emitting the variantIds to evaluate
	 * @returns {Observable<{ variantId: UmbVariantId; permitted: boolean }[]>} - Observable that emits an array of objects with a permitted boolean and the variantId
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	isPermittedForObservableVariants(
		variantIds: Observable<UmbVariantId[]>,
	): Observable<{ variantId: UmbVariantId; permitted: boolean }[]> {
		return mergeObservables([this.rules, variantIds, this._fallback], ([states, variantIds, fallback]) => {
			if (!variantIds || variantIds.length === 0) {
				return [];
			}
			return variantIds.map((id) => ({ variantId: id, permitted: this.#resolvePermission(states, id) ?? fallback }));
		});
	}

	/**
	 * Check if the given variantId is permitted to read
	 * @param {UmbVariantId} variantId
	 * @returns {boolean} - true if the variantId is permitted to read, false otherwise
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	getIsPermittedForVariant(variantId: UmbVariantId): boolean {
		return this.#resolvePermission(this.getRules(), variantId) ?? this._getFallback();
	}

	#resolvePermission(rules: UmbVariantGuardRule[], variantId: UmbVariantId): boolean | undefined {
		if (rules.filter((x) => x.permitted === false).some((rule) => findRule(rule, variantId))) {
			return false;
		}
		if (rules.filter((x) => x.permitted === true).some((rule) => findRule(rule, variantId))) {
			return true;
		}
		return undefined;
	}
}
