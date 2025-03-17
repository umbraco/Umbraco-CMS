import type { UmbStylesheetRule } from '../../types.js';
import { UMB_STYLESHEET_RULE_SETTINGS_MODAL } from './stylesheet-rule-settings-modal.token.js';
import { css, html, customElement, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-stylesheet-rule-input')
export class UmbStylesheetRuleInputElement extends UUIFormControlMixin(UmbLitElement, '') {
	#sorter = new UmbSorterController<UmbStylesheetRule>(this, {
		getUniqueOfElement: (element) => element.id,
		getUniqueOfModel: (modelEntry) => modelEntry.name,
		identifier: 'Umb.SorterIdentifier.InputStylesheetRule',
		itemSelector: 'umb-stylesheet-rule-ref',
		containerSelector: 'uui-ref-list',
		onChange: ({ model }) => {
			this.rules = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	@property({ type: Array, attribute: false })
	rules: UmbStylesheetRule[] = [];

	protected override getFormElement() {
		return undefined;
	}

	async #openRuleSettings(rule: UmbStylesheetRule | null = null) {
		return await umbOpenModal(this, UMB_STYLESHEET_RULE_SETTINGS_MODAL, {
			value: {
				rule: rule ? { name: rule.name, selector: rule.selector, styles: rule.styles } : null,
			},
		});
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

	override firstUpdated() {
		this.#sorter.setModel(this.rules);
	}

	override render() {
		return html`
			<uui-ref-list>
				${repeat(
					this.rules,
					(rule, index) => rule.name + index,
					(rule, index) => html`
						<umb-stylesheet-rule-ref
							name=${rule.name}
							id=${rule.name}
							detail=${rule.selector}
							@open=${() => this.#editRule(rule, index)}>
							<uui-action-bar slot="actions">
								<uui-button
									label=${this.localize.term('general_remove')}
									@click=${() => this.#removeRule(rule)}></uui-button>
							</uui-action-bar>
						</umb-stylesheet-rule-ref>
					`,
				)}
			</uui-ref-list>
			<uui-button label=${this.localize.term('general_add')} look="placeholder" @click=${this.#appendRule}></uui-button>
		`;
	}

	static override styles = [
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
