import type { UmbGuardRule } from './guard.manager.base.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbVariantGuardRule extends UmbGuardRule {
	variantId?: UmbVariantId;
}

/**
 * Checks if the given rule applies to the given variantId
 * @param {UmbVariantGuardRule} rule - The rule to check
 * @param {UmbVariantId} variantId - The variantId to check against
 * @returns {boolean} True if the rule applies to the variantId
 */
function findRule(rule: UmbVariantGuardRule, variantId: UmbVariantId) {
	return rule.variantId?.compare(variantId) || rule.variantId === undefined;
}

/**
 * Resolves the permission of a variantId from a set of variant rules. Rules that are not permitted take precedence.
 * @param {UmbVariantGuardRule[]} rules - The rules to resolve from
 * @param {UmbVariantId} variantId - The variantId to resolve the permission for
 * @returns {boolean | undefined} The resolved permission, or undefined if no rule applies to the variantId
 */
export function resolveVariantGuardPermission(
	rules: Array<UmbVariantGuardRule>,
	variantId: UmbVariantId,
): boolean | undefined {
	if (rules.filter((x) => x.permitted === false).some((rule) => findRule(rule, variantId))) {
		return false;
	}
	if (rules.filter((x) => x.permitted === true).some((rule) => findRule(rule, variantId))) {
		return true;
	}
	return undefined;
}
