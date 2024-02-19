import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from '../../types.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';

//import './column-layouts/document-table-actions-column-layout.element.js';

@customElement('umb-document-table-collection-view')
export class UmbDocumentTableCollectionViewElement extends UmbLitElement {
	@state()
	private _busy = false;

	@state()
	private _items?: Array<UmbDocumentCollectionItemModel>;

	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: true,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Name',
			alias: 'entityName',
			allowSorting: true,
		},
		// TODO: actions should live in an UmbTable element when we have moved the current UmbTable to UUI.
		// {
		// 	name: 'Actions',
		// 	alias: 'entityActions',
		// 	elementName: 'umb-document-table-actions-column-layout',
		// 	width: '80px',
		// },
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	@state()
	private _selection: Array<string> = [];

	private _collectionContext?: UmbDefaultCollectionContext<
		UmbDocumentCollectionItemModel,
		UmbDocumentCollectionFilterModel
	>;

	constructor() {
		super();
		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this._collectionContext = instance;
			this._observeCollectionContext();
		});
	}

	private _observeCollectionContext() {
		if (!this._collectionContext) return;

		this.observe(this._collectionContext.items, (items) => {
			this._items = items;
			this.#createTableItems(this._items);
		});

		this.observe(this._collectionContext.selection.selection, (selection) => {
			this._selection = selection as string[];
		});
	}

	#createTableItems(items: Array<UmbDocumentCollectionItemModel>) {
		this._tableItems = items.map((item) => {
			if (!item.unique) throw new Error('Item id is missing.');
			return {
				id: item.unique,
				data: [
					{
						columnAlias: 'entityName',
						value: item.name || 'Unnamed Document',
					},
					// {
					// 	columnAlias: 'entityActions',
					// 	value: {
					// 		entityType: item.entityType,
					// 	},
					// },
				],
				icon: item.icon,
			};
		});
	}

	private _handleSelect(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this._collectionContext?.selection.setSelection(selection);
	}

	private _handleDeselect(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this._collectionContext?.selection.setSelection(selection);
	}

	private _handleOrdering(event: UmbTableOrderedEvent) {
		const table = event.target as UmbTableElement;
		const orderingColumn = table.orderingColumn;
		const orderingDesc = table.orderingDesc;
		console.log(`fetch media items, order column: ${orderingColumn}, desc: ${orderingDesc}`);
	}

	render() {
		if (this._busy) return html`<div class="container"><uui-loader></uui-loader></div>`;

		if (this._tableItems.length === 0)
			return html`<div class="container"><p>${this.localize.term('content_listViewNoItems')}</p></div>`;

		return html`
			<umb-table
				.config=${this._tableConfig}
				.columns=${this._tableColumns}
				.items=${this._tableItems}
				.selection=${this._selection}
				@selected="${this._handleSelect}"
				@deselected="${this._handleDeselect}"
				@ordered="${this._handleOrdering}"></umb-table>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				box-sizing: border-box;
				height: auto;
				width: 100%;
				padding: var(--uui-size-space-3) 0;
			}

			/* TODO: Should we have embedded padding in the table component? */
			umb-table {
				padding: 0; /* To fix the embedded padding in the table component. */
			}

			.container {
				display: flex;
				justify-content: center;
				align-items: center;
			}
		`,
	];
}

export default UmbDocumentTableCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-document-table': UmbDocumentTableCollectionViewElement;
	}
}
