import { UmbGuardManagerBase, type UmbGuardRule } from './guard.manager.base.js';

function CompareRules(rules: Array<UmbGuardRule>): boolean {
	const firstState = rules[0];
	if (firstState) {
		return firstState.permitted;
	}

	return false;
}

// TODO: Check the need for this one.
export class UmbReadOnlyGuardManager<RuleType extends UmbGuardRule> extends UmbGuardManagerBase<RuleType> {
	public readonly permitted = this._rules.asObservablePart((rules) => {
		return CompareRules(rules);
	});

	getPermitted(): boolean {
		return CompareRules(this.getRules());
	}
}
