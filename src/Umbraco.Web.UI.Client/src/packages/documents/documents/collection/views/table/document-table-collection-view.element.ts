import { UMB_DOCUMENT_COLLECTION_CONTEXT } from '../../document-collection.context-token.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from '../../types.js';
import { css, customElement, html, state, type PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbCollectionViewElementBase } from '@umbraco-cms/backoffice/collection';
import type { UmbCollectionColumnConfiguration } from '@umbraco-cms/backoffice/collection';
import type {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';

import './column-layouts/document-entity-actions-table-column-view.element.js';
import './column-layouts/document-table-column-name.element.js';
import './column-layouts/document-table-column-property-value.element.js';
import './column-layouts/document-table-column-state.element.js';

@customElement('umb-document-table-collection-view')
export class UmbDocumentTableCollectionViewElement extends UmbCollectionViewElementBase<UmbDocumentCollectionItemModel> {
	@state()
	private _userDefinedProperties?: Array<UmbCollectionColumnConfiguration>;

	@state()
	private _tableColumns: Array<UmbTableColumn> = [];

	#tableConfig: UmbTableConfig = { allowSelection: false, allowSelectAll: false, selectOnly: false };

	#systemColumns: Array<UmbTableColumn> = [
		{
			name: this.localize.term('general_name'),
			alias: 'name',
			elementName: 'umb-document-table-column-name',
			allowSorting: true,
		},
		{
			name: this.localize.term('content_publishStatus'),
			alias: 'state',
			elementName: 'umb-document-table-column-state',
			allowSorting: false,
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	@state()
	private _orderingColumn = '';

	@state()
	private _orderingDesc = false;

	#collectionContext?: typeof UMB_DOCUMENT_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
			collectionContext?.setupView(this);
			this.#observeCollectionContext();
		});
	}

	#observeCollectionContext() {
		if (!this.#collectionContext) return;

		this.observe(
			this.#collectionContext.userDefinedProperties,
			(userDefinedProperties) => {
				this._userDefinedProperties = userDefinedProperties;
			},
			'_observeUserDefinedProperties',
		);

		this.observe(
			this.#collectionContext.filter,
			(filter) => {
				const { orderBy, orderDirection } = filter as UmbDocumentCollectionFilterModel;
				this._orderingColumn = orderBy ?? '';
				this._orderingDesc = orderDirection === 'desc';
			},
			'_observeOrdering',
		);
	}

	override willUpdate(changedProperties: PropertyValues) {
		super.willUpdate(changedProperties);

		if (
			changedProperties.has('_selectable') ||
			changedProperties.has('_multiple') ||
			changedProperties.has('_selectOnly')
		) {
			this.#tableConfig = {
				allowSelection: this._selectable,
				allowSelectAll: this._multiple,
				selectOnly: this._selectOnly,
			};
		}

		// The rows carry their own selectability, so they are rebuilt when selection becomes available as well.
		if (
			changedProperties.has('_items') ||
			changedProperties.has('_userDefinedProperties') ||
			changedProperties.has('_selectable') ||
			changedProperties.has('_hideItemActions')
		) {
			this.#createTableHeadings();
			this.#createTableItems();
		}
	}

	#createTableHeadings() {
		if (this._userDefinedProperties && this._userDefinedProperties.length > 0) {
			const userColumns: Array<UmbTableColumn> = this._userDefinedProperties.map((item) => {
				return {
					name: this.localize.string(item.header),
					alias: item.alias,
					elementName: item.elementName || 'umb-document-table-column-property-value',
					labelTemplate: item.nameTemplate,
					allowSorting: true,
					clipText: true,
				};
			});

			this._tableColumns = [
				...this.#systemColumns,
				...userColumns,
				...(this._hideItemActions ? [] : [{ name: '', alias: 'entityActions', align: 'right' } as UmbTableColumn]),
			];
		}
	}

	#createTableItems() {
		this._tableItems = this._items.map((item) => {
			if (!item.unique) throw new Error('Item id is missing.');

			const data =
				this._tableColumns?.map((column) => {
					if (column.alias === 'entityActions') {
						return {
							columnAlias: 'entityActions',
							value: html`<umb-document-entity-actions-table-column-view
								.value=${item}></umb-document-entity-actions-table-column-view>`,
						};
					}

					const editPath = UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
						unique: item.unique,
					});

					return {
						columnAlias: column.alias,
						value: { item, editPath },
					};
				}) ?? [];

			return {
				id: item.unique,
				icon: item.documentType.icon,
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
				selectable: this._isSelectableItem(item),
				data: data,
			};
		});
	}

	#onSelected(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const itemId = event.getItemId();

		// We get the same event for both single and multiple selection.
		if (itemId) {
			this._selectItem(itemId);
		} else {
			const target = event.target as UmbTableElement;
			this._setSelection(target.selection);
		}
	}

	#onDeselected(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const itemId = event.getItemId();

		// We get the same event for both single and multiple deselection.
		if (itemId) {
			this._deselectItem(itemId);
		} else {
			const target = event.target as UmbTableElement;
			this._setSelection(target.selection);
		}
	}

	#onOrdering(event: UmbTableOrderedEvent) {
		const table = event.target as UmbTableElement;
		const orderingColumn = table.orderingColumn;
		const orderingDesc = table.orderingDesc;
		this.#collectionContext?.setFilter({
			orderBy: orderingColumn,
			orderDirection: orderingDesc ? 'desc' : 'asc',
		});
	}

	override render() {
		return html`
			<umb-table
				.config=${this.#tableConfig}
				.columns=${this._tableColumns}
				.items=${this._tableItems}
				.selection=${this._selection}
				.orderingColumn=${this._orderingColumn}
				.orderingDesc=${this._orderingDesc}
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}
				@ordered=${this.#onOrdering}></umb-table>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				box-sizing: border-box;
				height: auto;
				width: 100%;
				padding: var(--uui-size-space-3) 0;
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
		'umb-document-table-collection-view': UmbDocumentTableCollectionViewElement;
	}
}
