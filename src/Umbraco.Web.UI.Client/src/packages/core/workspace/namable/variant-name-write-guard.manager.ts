import { UmbNameWriteGuardManager } from './name-write-guard.manager.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';
import { resolveVariantGuardPermission, type UmbVariantGuardRule } from '@umbraco-cms/backoffice/utils';

export type UmbVariantNameWriteGuardRule = UmbVariantGuardRule;

/**
 * Name write guard for variant workspaces. A rule without a `variantId` applies to every variant, and a rule that is
 * not permitted takes precedence. Without a `variantId` the guard answers like {@link UmbNameWriteGuardManager}:
 * only rules that are not bound to a variant apply.
 * @class UmbVariantNameWriteGuardManager
 * @augments {UmbNameWriteGuardManager<UmbVariantNameWriteGuardRule>}
 */
export class UmbVariantNameWriteGuardManager extends UmbNameWriteGuardManager<UmbVariantNameWriteGuardRule> {
	/**
	 * Observe if the name is permitted to be written.
	 * @param {UmbVariantId} [variantId] - The variant to check. Omit to check the name outside of any variant.
	 * @returns {Observable<boolean>} - Observable that emits true if the name is permitted to be written, false otherwise
	 * @memberof UmbVariantNameWriteGuardManager
	 */
	override isPermittedForName(variantId?: UmbVariantId): Observable<boolean> {
		return mergeObservables([this.rules, this._fallback], ([rules, fallback]) => {
			return this.#resolvePermission(rules, variantId) ?? fallback;
		});
	}

	/**
	 * Check if the name is permitted to be written.
	 * @param {UmbVariantId} [variantId] - The variant to check. Omit to check the name outside of any variant.
	 * @returns {boolean} - true if the name is permitted to be written, false otherwise
	 * @memberof UmbVariantNameWriteGuardManager
	 */
	override getIsPermittedForName(variantId?: UmbVariantId): boolean {
		return this.#resolvePermission(this.getRules(), variantId) ?? this._getFallback();
	}

	#resolvePermission(rules: Array<UmbVariantNameWriteGuardRule>, variantId?: UmbVariantId): boolean | undefined {
		if (variantId) {
			return resolveVariantGuardPermission(rules, variantId);
		}

		const unboundRules = rules.filter((rule) => rule.variantId === undefined);
		if (unboundRules.some((rule) => rule.permitted === false)) return false;
		if (unboundRules.some((rule) => rule.permitted === true)) return true;
		return undefined;
	}
}
