import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import { UmbGuardManagerBase, type UmbGuardRuleEntry } from './guard.manager.base.js';

function CompareRules(rules: Array<UmbGuardRuleEntry>): boolean {
	const firstState = rules[0];
	if (firstState) {
		return firstState.permitted;
	}

	return false;
}

export class UmbReadOnlyGuardManager<RuleType extends UmbGuardRuleEntry> extends UmbGuardManagerBase<RuleType> {
	public readonly isOn = this._rules.asObservablePart((rules) => {
		return CompareRules(rules);
	});

	getIsOn(): boolean {
		return CompareRules(this.getRules());
	}
}
