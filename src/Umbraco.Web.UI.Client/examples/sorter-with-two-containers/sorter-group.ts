import type ExampleSorterItem from './sorter-item.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';

import './sorter-item.js';

export type ModelEntryType = {
	name: string;
};

@customElement('example-sorter-group')
export class ExampleSorterGroup extends UmbElementMixin(LitElement) {
	//
	// Property that is used to set the model of the sorter.
	@property({ type: Array, attribute: false })
	public get items(): ModelEntryType[] {
		return this._items ?? [];
	}
	public set items(value: ModelEntryType[]) {
		// Only want to set the model initially via this one, as this is the initial model, cause any re-render should not set this data again.
		if (this._items !== undefined) return;
		this._items = value;
		this.#sorter.setModel(this._items);
	}
	private _items?: ModelEntryType[];

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
			const oldValue = this._items;
			this._items = model;
			this.requestUpdate('items', oldValue);
		},
	});

	removeItem = (item: ModelEntryType) => {
		this._items = this._items!.filter((r) => r.name !== item.name);
		this.#sorter.setModel(this._items);
	};

	override render() {
		return html`
			<div class="sorter-container">
				${repeat(
					this.items,
					(item) => item.name,
					(item) =>
						html`<example-sorter-item name=${item.name}>
							<button slot="action" @click=${() => this.removeItem(item)}>Delete</button>
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
		'example-sorter-group': ExampleSorterGroup;
	}
}
