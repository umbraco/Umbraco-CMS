import { UmbSorterConfig, UmbSorterController } from '../sorter.controller.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';

type SortEntryType = {
	id: string;
	value: string;
};

const sorterConfig: UmbSorterConfig<SortEntryType> = {
	compareElementToModel: (element: HTMLElement, model: SortEntryType) => {
		return element.getAttribute('data-sort-entry-id') === model.id;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: SortEntryType) => {
		return container.querySelector('data-sort-entry-id[' + modelEntry.id + ']');
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

	constructor() {
		super();

		this.sorter = new UmbSorterController(this, sorterConfig);
		this.sorter.setModel(model);
	}

	render() {
		return html`
			<ul>
				${model.map(
					(entry) => html`<li id="${'sort' + entry.id}" data-sort-entry-id="${entry.id}">${entry.value}</li>`
				)}
			</ul>
		`;
	}

	static styles = [
		css`
			:host {
				display: block;
			}

			ul {
				list-style: none;
				padding: 0;
				margin: 0;
			}

			li {
				padding: 10px;
				margin: 5px;
				background: #eee;
			}

			li:hover {
				background: #ddd !important;
				cursor: move;
			}

			li:active {
				background: #ccc;
			}

			#sort0 {
				background: #f00;
			}

			#sort1 {
				background: #0f0;
			}

			#sort2 {
				background: #c9da10;
			}
		`,
	];
}
