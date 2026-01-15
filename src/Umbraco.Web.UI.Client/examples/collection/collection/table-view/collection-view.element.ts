import type { ExampleCollectionItemModel } from '../repository/types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('example-table-collection-view')
export class ExampleTableCollectionViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Name',
			alias: 'name',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext<ExampleCollectionItemModel>;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	#observeCollectionItems() {
		this.observe(
			this.#collectionContext?.items,
			(items) => this.#createTableItems(items),
			'umbCollectionItemsObserver',
		);
	}

	#createTableItems(items: Array<ExampleCollectionItemModel> | undefined) {
		if (!items) {
			this._tableItems = [];
			return;
		}

		this._tableItems = items.map((item) => {
			return {
				id: item.unique,
				icon: item.icon,
				data: [
					{
						columnAlias: 'name',
						value: item.name,
					},
				],
			};
		});
	}

	override render() {
		return html`
			<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

export { ExampleTableCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-table-collection-view': ExampleTableCollectionViewElement;
	}
}
