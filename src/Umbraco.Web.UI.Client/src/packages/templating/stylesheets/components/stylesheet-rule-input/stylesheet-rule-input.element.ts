import type { UmbStylesheetRule } from '../../types.js';
import { UMB_STYLESHEET_RULE_SETTINGS_MODAL } from './stylesheet-rule-settings-modal.token.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

// TODO: add sorting when we have a generic sorting component/functionality for ref lists

@customElement('umb-stylesheet-rule-input')
export class UmbStylesheetRuleInputElement extends UUIFormControlMixin(UmbLitElement, '') {
	@property({ type: Array, attribute: false })
	rules: UmbStylesheetRule[] = [];

	protected getFormElement() {
		return undefined;
	}

	async #openRuleSettings(rule: UmbStylesheetRule | null = null) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);

		const value = {
			rule: rule ? { name: rule.name, selector: rule.selector, styles: rule.styles } : null,
		};

		const modalContext = modalManager.open(this, UMB_STYLESHEET_RULE_SETTINGS_MODAL, {
			value,
		});

		return modalContext?.onSubmit();
	}

	#appendRule = () => {
		this.#openRuleSettings(null)
			.then((value) => {
				if (!value.rule) return;
				this.rules = [...this.rules, value.rule];
				this.dispatchEvent(new UmbChangeEvent());
			})
			.catch(() => undefined);
	};

	#editRule = (rule: UmbStylesheetRule, index: number) => {
		this.#openRuleSettings(rule)
			.then((value) => {
				if (!value.rule) return;
				this.rules[index] = value.rule;
				this.dispatchEvent(new UmbChangeEvent());
				this.requestUpdate();
			})
			.catch(() => undefined);
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
			<uui-button label="Add rule" look="placeholder" @click=${this.#appendRule}>Add</uui-button>
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
