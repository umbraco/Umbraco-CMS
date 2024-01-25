import type { UmbStylesheetRule } from '../../types.js';
import { UMB_STYLESHEET_RULE_SETTINGS_MODAL } from './stylesheet-rule-settings-modal.token.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

// TODO: add sorting when we have a generic sorting component/functionality for ref lists

@customElement('umb-stylesheet-rule-input')
export class UmbStylesheetRuleInputElement extends FormControlMixin(UmbLitElement) {
	@property({ type: Array, attribute: false })
	rules: UmbStylesheetRule[] = [];

	#modalManager: UmbModalManagerContext | undefined;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (modalContext) => {
			this.#modalManager = modalContext;
		});
	}

	protected getFormElement() {
		return undefined;
	}

	#openRuleSettings = (rule: UmbStylesheetRule | null = null) => {
		if (!this.#modalManager) throw new Error('Modal context not found');

		const value = {
			rule: rule ? { name: rule.name, selector: rule.selector, styles: rule.styles } : null,
		};

		const modalContext = this.#modalManager.open(UMB_STYLESHEET_RULE_SETTINGS_MODAL, {
			value,
		});

		return modalContext?.onSubmit();
	};

	#appendRule = async () => {
		const { rule: newRule } = await this.#openRuleSettings(null);
		if (!newRule) return;
		this.rules = [...this.rules, newRule];
		this.dispatchEvent(new UmbChangeEvent());
	};

	#editRule = async (rule: UmbStylesheetRule, index: number) => {
		const { rule: updatedRule } = await this.#openRuleSettings(rule);
		if (!updatedRule) return;
		this.rules[index] = updatedRule;
		this.dispatchEvent(new UmbChangeEvent());
		this.requestUpdate();
	};

	#removeRule = (rule: UmbStylesheetRule) => {
		this.rules = this.rules.filter((r) => r.name !== rule.name);
		this.dispatchEvent(new UmbChangeEvent());
	};

	render() {
		return html`
			<uui-ref-list>
				${repeat(
					this.rules,
					(rule, index) => rule.name + index,
					(rule, index) => html`
						<umb-stylesheet-rule-ref name=${rule.name} detail=${rule.selector}>
							<uui-action-bar slot="actions">
								<uui-button @click=${() => this.#editRule(rule, index)} label="Edit ${rule.name}">Edit</uui-button>
								<uui-button @click=${() => this.#removeRule(rule)} label="Remove ${rule.name}">Remove</uui-button>
							</uui-action-bar>
						</umb-stylesheet-rule-ref>
					`,
				)}
			</uui-ref-list>
			<uui-button label="Add rule" look="placeholder" @click=${() => this.#appendRule()}>Add</uui-button>
		`;
	}

	static styles = [
		css`
			:host {
				display: block;
			}

			uui-button {
				display: block;
			}
		`,
	];
}

export default UmbStylesheetRuleInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-rule-input': UmbStylesheetRuleInputElement;
	}
}
