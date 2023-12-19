import { UmbStylesheetRule } from '../../types.js';
import { UMB_STYLESHEET_RULE_SETTINGS_MODAL } from './stylesheet-rule-settings-modal.token.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, ifDefined, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-stylesheet-rule-input')
export class UmbStylesheetRuleInputElement extends FormControlMixin(UmbLitElement) {
	@property({ type: Array, attribute: false })
	rules: UmbStylesheetRule[] = [];

	#modalManager: UmbModalManagerContext | undefined;

	/*
	#sorter = new UmbSorterController(this, {
		...SORTER_CONFIG,
		performItemInsert: ({ item, newIndex }) => {
			return this.#findNewSortOrder(item, newIndex) ?? false;
		},
		performItemRemove: () => {
			//defined so the default does not run
			return true;
		},
	});
	*/

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (modalContext) => {
			this.#modalManager = modalContext;
		});

		//this.#sorter.setModel(this._rules);
	}

	protected getFormElement() {
		return undefined;
	}

	#findNewSortOrder(rule: UmbStylesheetRule, newIndex: number) {
		const rules = [...this.getRules()].sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0));
		const oldIndex = rules.findIndex((r) => r.name === rule.name);

		if (oldIndex === -1) return false;
		rules.splice(oldIndex, 1);
		rules.splice(newIndex, 0, rule);
		this.setRules(rules.map((r, i) => ({ ...r, sortOrder: i })));
		return true;
	}

	#openRuleSettings = (rule: UmbStylesheetRule | null = null) => {
		if (!this.#modalManager) throw new Error('Modal context not found');
		const modalContext = this.#modalManager.open(UMB_STYLESHEET_RULE_SETTINGS_MODAL, {
			value: {
				rule,
			},
		});

		modalContext?.onSubmit().then((value) => {
			const newRule: UmbStylesheetRule = { ...value.rule };
			this.rules = [...this.rules, newRule];
			this.dispatchEvent(new UmbChangeEvent());
		});
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
					(rule) => html`
						<umb-stylesheet-rule-ref
							name=${rule.name}
							detail=${rule.selector}
							data-umb-rule-name="${ifDefined(rule?.name)}">
							<uui-action-bar slot="actions">
								<uui-button @click=${() => this.#openRuleSettings(rule)} label="Edit ${rule.name}">Edit</uui-button>
								<uui-button @click=${() => this.#removeRule(rule)} label="Remove ${rule.name}">Remove</uui-button>
							</uui-action-bar>
						</umb-stylesheet-rule-ref>
					`,
				)}
			</uui-ref-list>
			<uui-button label="Add rule" look="placeholder" @click=${() => this.#openRuleSettings(null)}>Add</uui-button>
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

const SORTER_CONFIG: UmbSorterConfig<UmbStylesheetRule> = {
	compareElementToModel: (element: HTMLElement, model: UmbStylesheetRule) => {
		return element.getAttribute('data-umb-rule-name') === model.name;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: UmbStylesheetRule) => {
		return container.querySelector('data-umb-rule-name[' + modelEntry.name + ']');
	},
	identifier: 'stylesheet-rules-sorter',
	itemSelector: 'umb-stylesheet-rich-text-editor-rule',
	containerSelector: '#rules-container',
};
