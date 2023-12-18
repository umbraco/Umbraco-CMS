import { UmbSortableStylesheetRule } from '../../types.js';
import { UMB_STYLESHEET_RULE_SETTINGS_MODAL } from './stylesheet-rule-settings-modal.token.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, ifDefined, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, UUIComboboxElement, UUIComboboxEvent } from '@umbraco-cms/backoffice/external/uui';
import { UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-stylesheet-rule-input')
export class UmbStylesheetRuleInputElement extends FormControlMixin(UmbLitElement) {
	@property({ type: Array, attribute: false })
	rules: UmbSortableStylesheetRule[] = [];

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

	#findNewSortOrder(rule: UmbSortableStylesheetRule, newIndex: number) {
		const rules = [...this.getRules()].sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0));
		const oldIndex = rules.findIndex((r) => r.name === rule.name);

		if (oldIndex === -1) return false;
		rules.splice(oldIndex, 1);
		rules.splice(newIndex, 0, rule);
		this.setRules(rules.map((r, i) => ({ ...r, sortOrder: i })));
		return true;
	}

	setRules(rules: UmbSortableStylesheetRule[]) {
		/*
		const newRules = rules.map((r, i) => ({ ...r, sortOrder: i }));
		this.#rules.next(newRules);
		this.sendRulesGetContent();
		*/
	}

	#onChange(event: UUIComboboxEvent) {
		event.stopPropagation();
		const target = event.target as UUIComboboxElement;

		if (typeof target?.value === 'string') {
			this.value = target.value;
			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	addRule = (rule: UmbSortableStylesheetRule | null = null) => {
		if (!this.#modalManager) throw new Error('Modal context not found');
		const modalContext = this.#modalManager.open(UMB_STYLESHEET_RULE_SETTINGS_MODAL, {
			value: {
				rule,
			},
		});

		modalContext?.onSubmit().then((value) => {
			console.log(value);
			/*
			if (result.rule) {
				console.log(result.rule);
				//this.#context?.setRules([...this._rules, { ...result.rule, sortOrder: this._rules.length }]);
			}
			*/
		});
	};

	removeRule = (rule: UmbSortableStylesheetRule) => {
		//const rules = this._rules?.filter((r) => r.name !== rule.name);
	};

	render() {
		return html`
			<uui-ref-list>
				${repeat(
					this.rules,
					(rule) => rule.name + rule.sortOrder,
					(rule) => html`
						<umb-stylesheet-rule-ref
							name=${rule.name}
							detail=${rule.selector}
							data-umb-rule-name="${ifDefined(rule?.name)}"></umb-stylesheet-rule-ref>
					`,
				)}
			</uui-ref-list>
			<uui-button label="Add rule" look="placeholder" @click=${() => this.addRule(null)}>Add</uui-button>
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

const SORTER_CONFIG: UmbSorterConfig<UmbSortableStylesheetRule> = {
	compareElementToModel: (element: HTMLElement, model: UmbSortableStylesheetRule) => {
		return element.getAttribute('data-umb-rule-name') === model.name;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: UmbSortableStylesheetRule) => {
		return container.querySelector('data-umb-rule-name[' + modelEntry.name + ']');
	},
	identifier: 'stylesheet-rules-sorter',
	itemSelector: 'umb-stylesheet-rich-text-editor-rule',
	containerSelector: '#rules-container',
};
