import { UmbGuardManagerBase } from './guard.manager.base.js';
import {
	resolveVariantGuardPermission,
	type UmbVariantGuardRule,
} from './resolve-variant-guard-permission.function.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';

/**
 * Guard manager for variant rules, where a permitted rule grants the guarded capability for the variant.
 * @class UmbVariantGuardManager
 * @augments {UmbGuardManagerBase<UmbVariantGuardRule>}
 */
export class UmbVariantGuardManager extends UmbGuardManagerBase<UmbVariantGuardRule> {
	/**
	 * Observe if the given variantId is permitted
	 * @param {UmbVariantId} variantId - The variantId to check
	 * @returns {Observable<boolean>} - Observable that emits true if the variantId is permitted, false otherwise
	 * @memberof UmbVariantGuardManager
	 */
	isPermittedForVariant(variantId: UmbVariantId): Observable<boolean> {
		return mergeObservables(
			[this._rules.asObservablePart((rules) => resolveVariantGuardPermission(rules, variantId)), this._fallback],
			([permitted, fallback]) => permitted ?? fallback,
		);
	}

	/**
	 * Check if the given variantId is permitted
	 * @param {UmbVariantId} variantId - The variantId to check
	 * @returns {boolean} - true if the variantId is permitted, false otherwise
	 * @memberof UmbVariantGuardManager
	 */
	getIsPermittedForVariant(variantId: UmbVariantId): boolean {
		return resolveVariantGuardPermission(this.getRules(), variantId) ?? this._getFallback();
	}
}
