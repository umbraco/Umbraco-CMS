import { UmbSorterConfig, UmbSorterController } from '../sorter.controller.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';

type SortEntryType = {
	id: string;
	value: string;
};

const verticalConfig: UmbSorterConfig<SortEntryType> = {
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

const horizontalConfig: UmbSorterConfig<SortEntryType> = {
	...verticalConfig,
	resolveVerticalDirection: () => false,
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

	constructor() {
		super();
		this.sorter = new UmbSorterController(this, verticalConfig);
		this.sorter.setModel(model);
	}

	#change() {
		this.sorter.destroy();

		if (this.vertical) {
			this.sorter = new UmbSorterController(this, horizontalConfig);
			this.sorter.setModel(model);
		} else {
			this.sorter = new UmbSorterController(this, verticalConfig);
			this.sorter.setModel(model);
		}

		this.vertical = !this.vertical;
	}

	render() {
		return html`
			<uui-button label="Change direction" look="outline" color="positive" @click=${this.#change}>
				Horizontal/Vertical
			</uui-button>
			<ul class="${this.vertical ? 'vertical' : 'horizontal'}">
				${model.map(
					(entry) =>
						html`<li class="item" id="${'sort' + entry.id}" data-sort-entry-id="${entry.id}">
							<span>${entry.value}</span>
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
				position: relative;
				flex: 1;
				padding: 10px;
				background-color: rgba(0, 255, 0, 0.3);
			}

			li:hover {
				background-color: rgba(0, 255, 0, 0.8);
				cursor: grab;
			}

			.--umb-sorter-placeholder > span {
				display: none;
			}

			ul.vertical li.--umb-sorter-placeholder {
				padding: 0;
			}

			ul.vertical .--umb-sorter-placeholder {
				max-height: 2px;
			}

			ul.vertical .--umb-sorter-placeholder::after {
				content: '';
				display: block;
				border-top: 2px solid blue;
			}

			ul.horizontal .--umb-sorter-placeholder {
				background-color: transparent;
			}

			ul.horizontal .--umb-sorter-placeholder::before {
				content: '';
				position: absolute;
				inset: 0;
				box-sizing: border-box;
				border: 2px dashed blue;
			}
		`,
	];
}
