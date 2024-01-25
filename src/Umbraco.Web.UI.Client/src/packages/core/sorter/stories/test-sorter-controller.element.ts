import type { UmbSorterConfig} from '../sorter.controller.js';
import { UmbSorterController } from '../sorter.controller.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';

type SortEntryType = {
	id: string;
	value: string;
};

const SORTER_CONFIG: UmbSorterConfig<SortEntryType> = {
	compareElementToModel: (element: HTMLElement, model: SortEntryType) => {
		return element.getAttribute('data-sort-entry-id') === model.id;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: SortEntryType) => {
		return container.querySelector('[data-sort-entry-id=' + modelEntry.id + ']');
	},
	identifier: 'test-sorter',
	itemSelector: 'li',
	containerSelector: 'ul',
};
const model: Array<SortEntryType> = [
	{
		id: '0',
		value: 'Entry 0',
	},
	{
		id: '1',
		value: 'Entry 1',
	},
	{
		id: '2',
		value: 'Entry 2',
	},
];

@customElement('test-my-sorter-controller')
export default class UmbTestSorterControllerElement extends UmbLitElement {
	public sorter;

	@state()
	private vertical = true;

	@state()
	private _items: Array<SortEntryType> = [...model];

	constructor() {
		super();
		this.sorter = new UmbSorterController(this, {
			...SORTER_CONFIG,
			resolveVerticalDirection: () => {
				this.vertical ? true : false;
			},
			onChange: ({ model }) => {
				const oldValue = this._items;
				this._items = model;
				this.requestUpdate('_items', oldValue);
			},
		});
		this.sorter.setModel(model);
	}

	#toggle() {
		this.vertical = !this.vertical;
	}

	render() {
		return html`
			<uui-button label="Change direction" look="outline" color="positive" @click=${this.#toggle}>
				Horizontal/Vertical
			</uui-button>
			<ul class="${this.vertical ? 'vertical' : 'horizontal'}">
				${this._items.map(
					(entry) =>
						html`<li class="item" data-sort-entry-id="${entry.id}">
							<span><uui-icon name="icon-wand"></uui-icon>${entry.value}</span>
						</li>`,
				)}
			</ul>
		`;
	}

	static styles = [
		css`
			:host {
				display: block;
				box-sizing: border-box;
			}

			ul {
				display: flex;
				flex-direction: column;
				gap: 5px;
				list-style: none;
				padding: 0;
				margin: 10px 0;
			}

			ul.horizontal {
				flex-direction: row;
			}

			li {
				cursor: grab;
				position: relative;
				flex: 1;
				border-radius: var(--uui-border-radius);
			}

			li span {
				display: flex;
				align-items: center;
				gap: 5px;
				padding: 10px;
				background-color: rgba(0, 255, 0, 0.3);
			}

			li.--umb-sorter-placeholder span {
				visibility: hidden;
			}

			li.--umb-sorter-placeholder::after {
				content: '';
				position: absolute;
				inset: 0px;
				border-radius: var(--uui-border-radius);
				border: 1px dashed var(--uui-color-divider-emphasis);
			}
		`,
	];
}
