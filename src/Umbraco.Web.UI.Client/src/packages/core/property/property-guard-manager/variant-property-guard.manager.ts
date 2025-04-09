import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyGuardRule } from './property-guard.manager';
import { UmbGuardManagerBase } from '../../utils/guard-manager';

export interface UmbVariantPropertyGuardRule extends UmbPropertyGuardRule {
	variantId?: UmbVariantId;
}

function CompareVariantAndPropertyWithStates(
	rules: UmbVariantPropertyGuardRule[],
	variantId: UmbVariantId,
	propertyType: UmbReferenceByUnique,
) {
	// any specific states for the variant and propertyType?
	const variantAndPropertyState = rules.find(
		(s) => s.variantId?.compare(variantId) && s.propertyType?.unique === propertyType.unique,
	);
	if (variantAndPropertyState) {
		return variantAndPropertyState.permitted;
	}

	// any specific states for the propertyType?
	const propertyState = rules.find((s) => s.variantId === undefined && s.propertyType?.unique === propertyType.unique);
	if (propertyState) {
		return propertyState.permitted;
	}

	// any specific states for the variant?
	const variantState = rules.find((s) => s.variantId?.compare(variantId) && s.propertyType === undefined);
	if (variantState) {
		return variantState.permitted;
	}

	// any state without variant:
	const nonVariantState = rules.find((s) => s.variantId === undefined && s.propertyType === undefined);
	if (nonVariantState) {
		return nonVariantState.permitted;
	}

	return false;
}

export class UmbVariantPropertyGuardManager extends UmbGuardManagerBase<UmbVariantPropertyGuardRule> {
	//
	permittedForVariantAndProperty(variantId: UmbVariantId, propertyType: UmbReferenceByUnique): Observable<boolean> {
		return this._rules.asObservablePart((rules) => CompareVariantAndPropertyWithStates(rules, variantId, propertyType));
	}

	/*
	isPermittedForVariantObservableAndProperty(
		variantId: Observable<UmbVariantId | undefined>,
		propertyType: UmbReferenceByUnique,
	): Observable<boolean> {
		return mergeObservables([this._rulesObservable, variantId], ([states, variantId]) => {
			if (!variantId) {
				// Or should we know about the fallback state here? [NL]
				return false;
			}
			return CompareVariantAndPropertyWithStates(states, variantId, propertyType);
		});
	}
	*/

	getPermittedForVariant(variantId: UmbVariantId, propertyType: UmbReferenceByUnique): boolean {
		return CompareVariantAndPropertyWithStates(this._rules.getValue(), variantId, propertyType);
	}
}
