import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';

import './sorter-item.js';
import ExampleSorterItem from './sorter-item.js';

export type ModelEntryType = {
	name: string;
	children?: ModelEntryType[];
};

@customElement('example-sorter-group')
export class ExampleSorterGroup extends UmbElementMixin(LitElement) {
	@property({ type: Array, attribute: false })
	public get items(): ModelEntryType[] {
		return this._items ?? [];
	}
	public set items(value: ModelEntryType[]) {
		// Only want to set the model initially, cause any re-render should not set this again.
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
		this.items = this._items!.filter((r) => r.name !== item.name);
	};

	render() {
		return html`
			<div class="sorter-container">
				${repeat(
					this.items,
					(item) => item.name,
					(item) =>
						html`<example-sorter-item name=${item.name}>
							<button slot="action" @click=${() => this.removeItem(item)}>Delete</button>
							${item.children && item.children.length > 0
								? html`<example-sorter-group .items=${item.children}></example-sorter-group>`
								: ''}
						</example-sorter-item>`,
				)}
			</div>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
			}

			.sorter-placeholder {
				opacity: 0.2;
			}

			example-sorter-group {
				display: block;
				width: 100%;
				min-height: 20px;
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-size-border-radius);
				padding: var(--uui-size-space-1);
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
