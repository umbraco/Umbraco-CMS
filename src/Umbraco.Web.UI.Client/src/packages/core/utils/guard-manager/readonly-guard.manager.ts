import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UmbGuardManagerBase, type UmbGuardRule } from './guard.manager.base.js';

// TODO: Check the need for this one.
export class UmbReadOnlyGuardManager<RuleType extends UmbGuardRule> extends UmbGuardManagerBase<RuleType> {
	public readonly permitted = mergeObservables(
		[
			this._rules.asObservablePart((rules) => {
				return this.#resolvePermission(rules);
			}),
			this._fallback,
		],
		([permitted, fallback]) => permitted ?? fallback,
	);

	getPermitted(): boolean {
		return this.#resolvePermission(this.getRules()) ?? this._getFallback();
	}

	#resolvePermission(rules: Array<UmbGuardRule>): boolean | undefined {
		if (rules.filter((x) => x.permitted === false).length > 0) {
			return false;
		}
		if (rules.filter((x) => x.permitted === true).length > 0) {
			return true;
		}

		return undefined;
	}
}
