import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';

import './sorter-item.js';
import ExampleSorterItem from './sorter-item.js';

export type ModelEntryType = {
	name: string;
	children?: ModelEntryType[];
};

@customElement('example-sorter-group')
export class ExampleSorterGroup extends UmbElementMixin(LitElement) {
	//
	// Sorter setup:
	#sorter = new UmbSorterController<ModelEntryType, ExampleSorterItem>(this, {
		getUniqueOfElement: (element) => {
			return element.name;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry.name;
		},
		identifier: 'string-that-identifies-all-example-sorters',
		itemSelector: 'example-sorter-item',
		containerSelector: '.sorter-container',
		onChange: ({ model }) => {
			const oldValue = this._value;
			this._value = model;
			this.requestUpdate('value', oldValue);
			// Fire an event for the parent to know that the model has changed.
			this.dispatchEvent(new CustomEvent('change'));
		},
	});

	@property({ type: Array, attribute: false })
	public get value(): ModelEntryType[] {
		return this._value ?? [];
	}
	public set value(value: ModelEntryType[]) {
		const oldValue = this._value;
		this._value = value;
		this.#sorter.setModel(this._value);
		this.requestUpdate('value', oldValue);
	}
	private _value?: ModelEntryType[];

	removeItem = (item: ModelEntryType) => {
		this.value = this.value.filter((r) => r.name !== item.name);
	};

	override render() {
		return html`
			<div class="sorter-container">
				${repeat(
					this.value,
					// Note: ideally you have an unique identifier for each item, but for this example we use the `name` as identifier.
					(item) => item.name,
					(item) =>
						html`<example-sorter-item name=${item.name}>
							<button slot="action" @click=${() => this.removeItem(item)}>Delete</button>
							<example-sorter-group
								.value=${item.children ?? []}
								@change=${(e: Event) => {
									item.children = (e.target as ExampleSorterGroup).value;
								}}></example-sorter-group>
						</example-sorter-item>`,
				)}
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				border: 1px dashed rgba(122, 122, 122, 0.25);
				border-radius: calc(var(--uui-border-radius) * 2);
				padding: var(--uui-size-space-1);
			}

			.sorter-placeholder {
				opacity: 0.2;
			}

			.sorter-container {
				min-height: 20px;
			}
		`,
	];
}

export default ExampleSorterGroup;

declare global {
	interface HTMLElementTagNameMap {
		'example-sorter-group-nested': ExampleSorterGroup;
	}
}
