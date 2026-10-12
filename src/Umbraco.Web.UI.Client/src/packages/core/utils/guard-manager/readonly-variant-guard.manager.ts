import { UmbReadOnlyGuardManager } from './readonly-guard.manager.js';
import {
	resolveVariantGuardPermission,
	type UmbVariantGuardRule,
} from './resolve-variant-guard-permission.function.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';

export type { UmbVariantGuardRule } from './resolve-variant-guard-permission.function.js';

/**
 * Read only guard manager for variant rules.
 * @class UmbReadOnlyVariantGuardManager
 * @augments {UmbReadOnlyGuardManager<UmbVariantGuardRule>}
 */
export class UmbReadOnlyVariantGuardManager extends UmbReadOnlyGuardManager<UmbVariantGuardRule> {
	/**
	 * Observe if the given variantId is permitted to read
	 * @param {UmbVariantId} variantId - The variantId to check
	 * @returns {Observable<boolean>} - Observable that emits true if the variantId is permitted to read, false otherwise
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	isPermittedForVariant(variantId: UmbVariantId): Observable<boolean> {
		return mergeObservables(
			[
				this._rules.asObservablePart((rules) => {
					return resolveVariantGuardPermission(rules, variantId);
				}),
				this._fallback,
			],
			([permitted, fallback]) => permitted ?? fallback,
		);
	}

	/**
	 * @param {Observable<UmbVariantId | undefined>} variantId - Observable emitting the variantId to evaluate
	 * @returns {Observable<boolean | undefined>} - Observable that emits true if the variantId is permitted to read, false otherwise
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	isPermittedForObservableVariant(variantId: Observable<UmbVariantId | undefined>): Observable<boolean | undefined> {
		return mergeObservables([this.rules, variantId, this._fallback], ([rules, variantId, fallback]) => {
			if (!variantId) {
				return undefined;
			}
			return resolveVariantGuardPermission(rules, variantId) ?? fallback;
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
		return mergeObservables([this.rules, variantIds, this._fallback], ([rules, variantIds, fallback]) => {
			if (!variantIds || variantIds.length === 0) {
				return [];
			}
			return variantIds.map((id) => ({
				variantId: id,
				permitted: resolveVariantGuardPermission(rules, id) ?? fallback,
			}));
		});
	}

	/**
	 * Check if the given variantId is permitted to read
	 * @param {UmbVariantId} variantId - The variantId to check
	 * @returns {boolean} - true if the variantId is permitted to read, false otherwise
	 * @memberof UmbReadOnlyVariantGuardManager
	 */
	getIsPermittedForVariant(variantId: UmbVariantId): boolean {
		return resolveVariantGuardPermission(this.getRules(), variantId) ?? this._getFallback();
	}
}
