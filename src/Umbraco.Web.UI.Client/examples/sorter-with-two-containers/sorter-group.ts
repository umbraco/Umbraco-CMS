import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';

import './sorter-item.js';

type ModelEntryType = {
	name: string;
};

const SORTER_CONFIG: UmbSorterConfig<ModelEntryType> = {
	compareElementToModel: (element: HTMLElement, model: ModelEntryType) => {
		return element.getAttribute('name') === model.name;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: ModelEntryType) => {
		return container.querySelector('name[' + modelEntry.name + ']');
	},
	identifier: 'string-that-identifies-all-example-sorters',
	itemSelector: 'example-sorter-item',
	containerSelector: '.sorter-container',
};

@customElement('example-sorter-group')
export class ExampleSorterGroup extends UmbElementMixin(LitElement) {
	@state()
	_items: ModelEntryType[] = [
		{
			name: 'Apple',
		},
		{
			name: 'Banana',
		},
		{
			name: 'Pear',
		},
		{
			name: 'Pineapple',
		},
		{
			name: 'Lemon',
		},
	];

	#sorter = new UmbSorterController(this, {
		...SORTER_CONFIG,
		performItemInsert: ({ item, newIndex }) => {
			this._items.splice(newIndex, 0, item);
			console.log('inserted', item.name, 'at', newIndex, '	', this._items.map((x) => x.name).join(', '));
			this.requestUpdate('_items');
			return true;
		},
		performItemRemove: ({ item }) => {
			const indexToMove = this._items.findIndex((x) => x.name === item.name);
			this._items.splice(indexToMove, 1);
			console.log('removed', item.name, 'at', indexToMove, '	', this._items.map((x) => x.name).join(', '));
			this.requestUpdate('_items');
			return true;
		},
	});

	constructor() {
		super();
		this.#sorter.setModel(this._items);
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
					(item) => item,
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
