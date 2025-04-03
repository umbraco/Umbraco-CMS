import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbPartialSome } from '../type';

export interface UmbGuardIncomingRuleBase {
	unique?: string | symbol;
	permitted?: boolean;
	message?: string;
}

export interface UmbGuardRule extends UmbGuardIncomingRuleBase {
	unique: string | symbol;
	permitted: boolean;
}

const DefaultRuleUnique = Symbol();

export class UmbGuardManagerBase<
	RuleType extends UmbGuardRule = UmbGuardRule,
	IncomingRuleType extends UmbGuardIncomingRuleBase = UmbPartialSome<RuleType, 'unique' | 'permitted'>,
> extends UmbControllerBase {
	//
	protected readonly _rules = new UmbArrayState<RuleType>([], (x) => x.unique).sortBy((a, b) =>
		// If default then it should be last, if not then sort by permitted
		a.unique === DefaultRuleUnique ? 1 : a.permitted === b.permitted ? 0 : a.permitted ? 1 : -1,
	);
	public readonly rules = this._rules.asObservable();
	public readonly hasRules = this._rules.asObservablePart((x) => x.length > 0);

	public fallbackToDisallowed() {
		this._rules.appendOne({ unique: DefaultRuleUnique, permitted: false } as RuleType);
	}

	public fallbackToPermitted() {
		this._rules.appendOne({ unique: DefaultRuleUnique, permitted: true } as RuleType);
	}

	/**
	 * Add a new rule
	 * @param {RuleType} rule
	 */
	addRule(rule: IncomingRuleType) {
		const newRule = { ...rule } as unknown as RuleType;
		rule.unique ??= Symbol();
		if (rule.permitted === undefined) {
			rule.permitted = true;
		}
		this._rules.appendOne(newRule);
		return rule.unique;
	}

	/**
	 * Add multiple rules
	 * @param {RuleType[]} rules
	 */
	addRules(rules: IncomingRuleType[]) {
		this._rules.mute();
		rules.forEach((rule) => this.addRule(rule));
		this._rules.unmute();
	}

	/**
	 * Remove a rule
	 * @param {RuleType['unique']} unique Unique value of the rule to remove
	 */
	removeRule(unique: RuleType['unique']) {
		this._rules.removeOne(unique);
	}

	/**
	 * Remove multiple rules
	 * @param {RuleType['unique'][]} uniques Array of unique values to remove
	 */
	removeRules(uniques: RuleType['unique'][]) {
		this._rules.remove(uniques);
	}

	/**
	 * Get all rules
	 * @returns {RuleType[]} Array of rules
	 */
	getRules(): RuleType[] {
		return this._rules.getValue();
	}

	/**
	 * Clear all rules
	 */
	clear(): void {
		this._rules.setValue([]);
	}

	override destroy() {
		this._rules.destroy();
		super.destroy();
	}
}
