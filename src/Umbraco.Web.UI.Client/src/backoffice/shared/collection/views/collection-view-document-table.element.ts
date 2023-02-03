import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbCollectionContext, UMB_COLLECTION_CONTEXT_TOKEN } from '../collection.context';
import {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSelectedEvent,
} from '../../components/table';
import type { DocumentDetails } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

type EntityType = DocumentDetails;

@customElement('umb-collection-view-document-table')
export class UmbCollectionViewDocumentTableElement extends UmbLitElement {

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

	@state()
	private _items?: Array<EntityType>;

	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: true,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Name',
			alias: 'entityName',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	@state()
	private _selection: Array<string> = [];

	private _collectionContext?: UmbCollectionContext<EntityType>;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this._collectionContext = instance;
			this._observeCollectionContext();
		});
	}

	private _observeCollectionContext() {
		if (!this._collectionContext) return;

		this.observe(this._collectionContext.data, (items) => {
			this._items = items;
			this._createTableItems(this._items);
		});

		this.observe(this._collectionContext.selection, (selection) => {
			this._selection = selection;
		});
	}

	private _createTableItems(items: Array<any>) {
		this._tableItems = items.map((item) => {
			return {
				key: item.key,
				icon: item.icon,
				data: [
					{
						columnAlias: 'entityName',
						value: item.name || 'Untitled',
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-document-table': UmbCollectionViewDocumentTableElement;
	}
}
