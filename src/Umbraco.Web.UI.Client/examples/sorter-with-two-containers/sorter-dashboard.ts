import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';

type ModelEntryType = {
	name: string;
};

const SORTER_CONFIG: UmbSorterConfig<ModelEntryType> = {
	compareElementToModel: (element: HTMLElement, model: ModelEntryType) => {
		return element.getAttribute('data-name') === model.name;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: ModelEntryType) => {
		return container.querySelector('data-name[' + modelEntry.name + ']');
	},
	identifier: 'string-that-identifies-all-example-sorters',
	itemSelector: '.sorter-item',
	containerSelector: '.sorter-container',
};

@customElement('example-sorter-dashboard')
export class ExampleSorterDashboard extends UmbElementMixin(LitElement) {
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
			this.requestUpdate('_items');
			return true;
		},
		performItemRemove: ({ item }) => {
			const indexToMove = this._items.findIndex((x) => x.name === item.name);
			this._items.splice(indexToMove, 1);
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
			<uui-box class="uui-text">
				<div class="sorter-container">
					${repeat(
						this._items,
						(item) => item.name,
						(item) =>
							html`<div class="sorter-item" data-name=${item.name}>
								${item.name} <button @click=${() => this.removeItem(item)}>Delete</button>
							</div>`,
					)}
				</div>
			</uui-box>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}

			.sorter-item {
				display: flex;
				align-items: center;
				justify-content: space-between;
				padding: var(--uui-size-layout-0);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				margin-bottom: 3px;
			}
		`,
	];
}

export default ExampleSorterDashboard;

declare global {
	interface HTMLElementTagNameMap {
		'example-sorter-dashboard': ExampleSorterDashboard;
	}
}
