import type { UmbCollectionColumnConfiguration } from '../../../../../core/collection/types.js';
import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from '../../types.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';

import './column-layouts/media-table-column-name.element.js';

@customElement('umb-media-table-collection-view')
export class UmbMediaTableCollectionViewElement extends UmbLitElement {
	@state()
	private _loading = false;

	@state()
	private _userDefinedProperties?: Array<UmbCollectionColumnConfiguration>;

	@state()
	private _items?: Array<UmbMediaCollectionItemModel>;

	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: true,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [];

	#systemColumns: Array<UmbTableColumn> = [
		{
			name: this.localize.term('general_name'),
			alias: 'entityName',
			elementName: 'umb-media-table-column-name',
			allowSorting: true,
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	@state()
	private _selection: Array<string> = [];

	#collectionContext?: UmbDefaultCollectionContext<UmbMediaCollectionItemModel, UmbMediaCollectionFilterModel>;

	constructor() {
		super();
		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
			this.#observeCollectionContext();
		});
	}

	#observeCollectionContext() {
		if (!this.#collectionContext) return;

		this.observe(
			this.#collectionContext.userDefinedProperties,
			(userDefinedProperties) => {
				this._userDefinedProperties = userDefinedProperties;
				this.#createTableHeadings();
			},
			'umbCollectionUserDefinedPropertiesObserver',
		);

		this.observe(
			this.#collectionContext.items,
			(items) => {
				this._items = items;
				this.#createTableItems(this._items);
			},
			'umbCollectionItemsObserver',
		);

		this.observe(
			this.#collectionContext.selection.selection,
			(selection) => {
				this._selection = selection as string[];
			},
			'umbCollectionSelectionObserver',
		);
	}

	#createTableHeadings() {
		if (this._userDefinedProperties && this._userDefinedProperties.length > 0) {
			const userColumns: Array<UmbTableColumn> = this._userDefinedProperties.map((item) => {
				return {
					name: item.header,
					alias: item.alias,
					elementName: item.elementName,
					allowSorting: true,
				};
			});

			this._tableColumns = [...this.#systemColumns, ...userColumns];
		} else {
			this._tableColumns = [...this.#systemColumns];
		}
	}

	#createTableItems(items: Array<UmbMediaCollectionItemModel>) {
		if (this._tableColumns.length === 0) {
			this.#createTableHeadings();
		}

		this._tableItems = items.map((item) => {
			if (!item.unique) throw new Error('Item id is missing.');

			const data =
				this._tableColumns?.map((column) => {
					return {
						columnAlias: column.alias,
						value: column.elementName ? item : this.#getPropertyValueByAlias(item, column.alias),
					};
				}) ?? [];

			return {
				id: item.unique,
				icon: item.icon,
				data: data,
			};
		});
	}

	#getPropertyValueByAlias(item: UmbMediaCollectionItemModel, alias: string) {
		switch (alias) {
			case 'createDate':
				return item.createDate.toLocaleString();
			case 'entityName':
				return item.name;
			case 'owner':
				return item.creator;
			case 'updateDate':
				return item.updateDate.toLocaleString();
			default:
				return item.values.find((value) => value.alias === alias)?.value ?? '';
		}
	}

	#handleSelect(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#collectionContext?.selection.setSelection(selection);
	}

	#handleDeselect(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#collectionContext?.selection.setSelection(selection);
	}

	#handleOrdering(event: UmbTableOrderedEvent) {
		const table = event.target as UmbTableElement;
		const orderingColumn = table.orderingColumn;
		const orderingDesc = table.orderingDesc;
		this.#collectionContext?.setFilter({
			orderBy: orderingColumn,
			orderDirection: orderingDesc ? 'desc' : 'asc',
		});
	}

	render() {
		if (this._loading) {
			return html`<div class="container"><uui-loader></uui-loader></div>`;
		}

		if (this._tableItems.length === 0) {
			return html`<div class="container"><p>${this.localize.term('content_listViewNoItems')}</p></div>`;
		}

		return html`
			<umb-table
				.config=${this._tableConfig}
				.columns=${this._tableColumns}
				.items=${this._tableItems}
				.selection=${this._selection}
				@selected="${this.#handleSelect}"
				@deselected="${this.#handleDeselect}"
				@ordered="${this.#handleOrdering}"></umb-table>
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
				padding: var(--uui-size-space-3) var(--uui-size-space-6);
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

export default UmbMediaTableCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-table-collection-view': UmbMediaTableCollectionViewElement;
	}
}
