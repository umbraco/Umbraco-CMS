import { UmbGuardManagerBase, type UmbGuardRule } from './guard.manager.base.js';

// TODO: Check the need for this one.
export class UmbReadOnlyGuardManager<RuleType extends UmbGuardRule> extends UmbGuardManagerBase<RuleType> {
	public readonly permitted = this._rules.asObservablePart((rules) => {
		return this.#resolvePermission(rules);
	});

	getPermitted(): boolean {
		return this.#resolvePermission(this.getRules());
	}

	#resolvePermission(rules: Array<UmbGuardRule>): boolean {
		if (rules.filter((x) => x.permitted === false).length > 0) {
			return false;
		}
		if (rules.filter((x) => x.permitted === true).length > 0) {
			return true;
		}

		return this._fallback;
	}
}
