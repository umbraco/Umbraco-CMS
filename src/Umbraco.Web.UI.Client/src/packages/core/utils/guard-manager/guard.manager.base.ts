import type { UmbPartialSome } from '../type/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export interface UmbGuardIncomingRuleBase {
	unique?: string | symbol;
	permitted?: boolean;
	message?: string;
}

export interface UmbGuardRule extends UmbGuardIncomingRuleBase {
	unique: string | symbol;
	permitted: boolean;
}

export abstract class UmbGuardManagerBase<
	RuleType extends UmbGuardRule = UmbGuardRule,
	IncomingRuleType extends UmbGuardIncomingRuleBase = UmbPartialSome<RuleType, 'unique' | 'permitted'>,
> extends UmbControllerBase {
	//
	protected readonly _rules = new UmbArrayState<RuleType>([], (x) => x.unique);
	public readonly rules = this._rules.asObservable();
	public readonly hasRules = this._rules.asObservablePart((x) => x.length > 0);

	#fallback = new UmbBooleanState(false);
	protected _fallback = this.#fallback.asObservable();
	protected _getFallback() {
		return this.#fallback.getValue();
	}

	public fallbackToNotPermitted() {
		this.#fallback.setValue(false);
	}

	public fallbackToPermitted() {
		this.#fallback.setValue(true);
	}

	/**
	 * Add a new rule
	 * @param {RuleType} rule
	 */
	addRule(rule: IncomingRuleType): RuleType['unique'] {
		const newRule = { ...rule } as unknown as RuleType;
		newRule.unique ??= Symbol();
		if (newRule.permitted === undefined) {
			newRule.permitted = true;
		}
		this._rules.appendOne(newRule);
		return newRule.unique;
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
	clearRules(): void {
		this._rules.setValue([]);
	}

	override destroy() {
		this._rules.destroy();
		super.destroy();
	}
}
