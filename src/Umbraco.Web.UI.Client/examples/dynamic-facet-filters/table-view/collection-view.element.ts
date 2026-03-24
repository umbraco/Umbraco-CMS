import type { ExampleProductCollectionItemModel } from '../collection/repository/types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('example-dynamic-facet-table-view')
export class ExampleDynamicFacetTableViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{ name: 'Name', alias: 'name' },
		{ name: 'Category', alias: 'category' },
		{ name: 'Sizes', alias: 'sizes' },
		{ name: 'Colors', alias: 'colors' },
		{ name: 'Price', alias: 'price' },
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext<ExampleProductCollectionItemModel>;

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

	#createTableItems(items: Array<ExampleProductCollectionItemModel> | undefined) {
		if (!items) {
			this._tableItems = [];
			return;
		}

		this._tableItems = items.map((item) => ({
			id: item.unique,
			icon: item.icon,
			data: [
				{ columnAlias: 'name', value: item.name },
				{ columnAlias: 'category', value: item.category },
				{ columnAlias: 'sizes', value: item.sizes.join(', ') },
				{ columnAlias: 'colors', value: item.colors.join(', ') },
				{ columnAlias: 'price', value: `$${item.price}` },
			],
		}));
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

export { ExampleDynamicFacetTableViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-dynamic-facet-table-view': ExampleDynamicFacetTableViewElement;
	}
}
