import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';

import './sorter-item.js';
import ExampleSorterItem from './sorter-item.js';

export type ModelEntryType = {
	name: string;
};

const SORTER_CONFIG: UmbSorterConfig<ModelEntryType, ExampleSorterItem> = {
	compareElementToModel: (element, model) => {
		return element.name === model.name;
	},
	querySelectModelToElement: (container, modelEntry) => {
		return container.querySelector("example-sorter-item[name='" + modelEntry.name + "']");
	},
	identifier: 'string-that-identifies-all-example-sorters',
	itemSelector: 'example-sorter-item',
	containerSelector: '.sorter-container',
};

@customElement('example-sorter-group')
export class ExampleSorterGroup extends UmbElementMixin(LitElement) {
	@property({ type: Array, attribute: false })
	public get items(): ModelEntryType[] {
		return this._items;
	}
	public set items(value: ModelEntryType[]) {
		this._items = value;
		this.#sorter.setModel(this._items);
	}
	private _items: ModelEntryType[] = [];

	#sorter = new UmbSorterController<ModelEntryType, ExampleSorterItem>(this, {
		...SORTER_CONFIG,
		performItemInsert: ({ item, newIndex }) => {
			this._items.splice(newIndex, 0, item);
			//console.log('inserted', item.name, 'at', newIndex, '	', this._items.map((x) => x.name).join(', '));
			this.requestUpdate('_items');
			return true;
		},
		performItemRemove: ({ item }) => {
			const indexToMove = this._items.findIndex((x) => x.name === item.name);
			this._items.splice(indexToMove, 1);
			//console.log('removed', item.name, 'at', indexToMove, '	', this._items.map((x) => x.name).join(', '));
			this.requestUpdate('_items');
			return true;
		},
	});

	constructor() {
		super();
	}

	removeItem = (item: ModelEntryType) => {
		this._items = this._items.filter((r) => r.name !== item.name);
		this.#sorter.setModel(this._items);
	};

	render() {
		return html`
			<div class="sorter-container">
				${repeat(
					this._items,
					(item, index) => item.name + '_ ' + index,
					(item) =>
						html`<example-sorter-item name=${item.name}>
							<button @click=${() => this.removeItem(item)}>Delete</button>
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
		`,
	];
}

export default ExampleSorterGroup;

declare global {
	interface HTMLElementTagNameMap {
		'example-sorter-group': ExampleSorterGroup;
	}
}
