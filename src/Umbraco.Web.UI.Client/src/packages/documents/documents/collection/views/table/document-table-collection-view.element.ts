import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';
import { UMB_COLLECTION_CONTEXT_TOKEN, UmbCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DocumentTreeItemResponseModel, EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

import './column-layouts/document-table-actions-column-layout.element';

type EntityType = DocumentTreeItemResponseModel;

@customElement('umb-document-table-collection-view')
export class UmbDocumentTableCollectionViewElement extends UmbLitElement {
	@state()
	private _items?: Array<EntityTreeItemResponseModel>;

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
		{
			name: 'Actions',
			alias: 'entityActions',
			elementName: 'umb-document-table-actions-column-layout',
			width: '80px',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	@state()
	private _selection: Array<string> = [];

	private _collectionContext?: UmbCollectionContext<EntityType, any>;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this._collectionContext = instance;
			this._observeCollectionContext();
		});
	}

	private _observeCollectionContext() {
		if (!this._collectionContext) return;

		this.observe(this._collectionContext.items, (items) => {
			this._items = items;
			this._createTableItems(this._items);
		});

		this.observe(this._collectionContext.selection, (selection) => {
			this._selection = selection;
		});
	}

	private _createTableItems(items: Array<EntityTreeItemResponseModel>) {
		this._tableItems = items.map((item) => {
			// TODO: use unique instead of id
			if (!item.id) throw new Error('Item id is missing.');
			return {
				id: item.id,
				icon: item.icon,
				data: [
					{
						columnAlias: 'entityName',
						value: item.name || 'Untitled',
					},
					{
						columnAlias: 'entityActions',
						value: {
							entityType: item.type,
						},
					},
				],
			};
		});
	}

	private _handleSelect(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this._collectionContext?.setSelection(selection);
	}

	private _handleDeselect(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this._collectionContext?.setSelection(selection);
	}

	private _handleOrdering(event: UmbTableOrderedEvent) {
		const table = event.target as UmbTableElement;
		const orderingColumn = table.orderingColumn;
		const orderingDesc = table.orderingDesc;
		console.log(`fetch media items, order column: ${orderingColumn}, desc: ${orderingDesc}`);
	}

	render() {
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
		UUITextStyles,
		css`
			:host {
				display: block;
				box-sizing: border-box;
				height: 100%;
				width: 100%;
				padding: var(--uui-size-space-3) var(--uui-size-space-6);
			}

			/* TODO: Should we have embedded padding in the table component? */
			umb-table {
				padding: 0; /* To fix the embedded padding in the table component. */
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
