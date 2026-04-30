import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbGuardManagerBase, type UmbGuardRule } from '@umbraco-cms/backoffice/utils';

export interface UmbVariantNameWriteGuardRule extends UmbGuardRule {
	variantId?: UmbVariantId;
}

function findRule(rule: UmbVariantNameWriteGuardRule, variantId: UmbVariantId) {
	return rule.variantId === undefined || rule.variantId.culture === variantId.culture;
}

export class UmbVariantNameWriteGuardManager extends UmbGuardManagerBase<UmbVariantNameWriteGuardRule> {
	isPermittedForVariantName(variantId: UmbVariantId): Observable<boolean> {
		return this._rules.asObservablePart((rules) => this.#resolvePermission(rules, variantId));
	}

	getIsPermittedForVariantName(variantId: UmbVariantId): boolean {
		return this.#resolvePermission(this._rules.getValue(), variantId);
	}

	#resolvePermission(rules: UmbVariantNameWriteGuardRule[], variantId: UmbVariantId): boolean {
		if (rules.filter((x) => x.permitted === false).some((rule) => findRule(rule, variantId))) {
			return false;
		}
		if (rules.filter((x) => x.permitted === true).some((rule) => findRule(rule, variantId))) {
			return true;
		}
		return this._fallback;
	}
}
