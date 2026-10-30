import { UMB_DOCUMENT_COLLECTION_CONTEXT } from '../../document-collection.context-token.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from '../../types.js';
import { css, customElement, html, state, type PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbCollectionViewElementBase } from '@umbraco-cms/backoffice/collection';
import type { UmbCollectionColumnConfiguration } from '@umbraco-cms/backoffice/collection';
import { UmbTreeItemOpenEvent } from '@umbraco-cms/backoffice/tree';
import { UmbElementControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbAncestorsEntityContext, UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbEntityContentTypeEntityContext } from '@umbraco-cms/backoffice/content-type';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import type {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';

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

	#rowContexts = new Map<
		string,
		{
			host: UmbElementControllerHost;
			entityContext: UmbEntityContext;
			entityContentTypeContext: UmbEntityContentTypeEntityContext;
			ancestorsContext: UmbAncestorsEntityContext;
		}
	>();

	#onRowRendered = (element: HTMLElement | undefined, row: UmbTableItem) => {
		if (!element) {
			const existing = this.#rowContexts.get(row.id);
			if (existing) {
				existing.host.destroy();
				this.#rowContexts.delete(row.id);
			}
			return;
		}

		let rowContexts = this.#rowContexts.get(row.id);
		if (!rowContexts) {
			const host = new UmbElementControllerHost(element);
			host.hostConnected();
			rowContexts = {
				host,
				entityContext: new UmbEntityContext(host),
				entityContentTypeContext: new UmbEntityContentTypeEntityContext(host),
				ancestorsContext: new UmbAncestorsEntityContext(host),
			};
			this.#rowContexts.set(row.id, rowContexts);
		}

		const item = this._items.find((x) => x.unique === row.id);
		const documentTypeUnique = item?.contentType?.unique;

		rowContexts.entityContext.setEntityType(row.entityType);
		rowContexts.entityContext.setUnique(row.id);
		rowContexts.entityContentTypeContext.setEntityType(documentTypeUnique ? UMB_DOCUMENT_TYPE_ENTITY_TYPE : undefined);
		rowContexts.entityContentTypeContext.setUnique(documentTypeUnique);
		rowContexts.ancestorsContext.setAncestors(item?.ancestors ?? []);
	};

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

		// The rows carry their own selectability and href, so they are rebuilt when selection or select-only
		// mode changes too - not just when the underlying items change.
		if (
			changedProperties.has('_items') ||
			changedProperties.has('_userDefinedProperties') ||
			changedProperties.has('_selectable') ||
			changedProperties.has('_selectOnly') ||
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
				...(this._hideItemActions
					? []
					: [
							{
								name: '',
								alias: 'entityActions',
								align: 'right',
								elementName: 'umb-entity-actions-table-column-view',
							} as UmbTableColumn,
						]),
			];
		}
	}

	#createTableItems() {
		this._tableItems = this._items.map((item) => {
			if (!item.unique) throw new Error('Item id is missing.');

			// While select-only (i.e. in a picker), the name must not navigate away from the picker. An item with
			// children still needs a way to be opened, so it drills further into it via an open event instead.
			const editPath = UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique });
			const href = this._selectOnly ? undefined : editPath;
			const hasCollection = !!item.contentType?.collection;
			const indicatorHref = href && hasCollection ? `${href}?openCollection=true` : href;
			const onOpen =
				item.hasChildren || hasCollection
					? () => this.dispatchEvent(new UmbTreeItemOpenEvent({ unique: item.unique, entityType: item.entityType }))
					: undefined;

			const data =
				this._tableColumns?.map((column) => {
					if (column.alias === 'entityActions') {
						return {
							columnAlias: 'entityActions',
							value: {},
						};
					}

					return {
						columnAlias: column.alias,
						value: { item, href, onOpen },
					};
				}) ?? [];

			return {
				id: item.unique,
				icon: item.documentType.icon,
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
				childrenIndicator:
					item.hasChildren || hasCollection
						? {
								href: indicatorHref,
								onOpen,
								renderExpandSymbol: hasCollection
									? () => html`<umb-icon name="icon-list" style="font-size: 8px;"></umb-icon>`
									: undefined,
							}
						: undefined,
				selectable: this._isSelectableItem(item),
				// select-only disables all row interaction, which would leave no way to open an item with
				// children while a selection is in progress.
				selectOnly: item.hasChildren || hasCollection ? false : undefined,
				data: data,
			};
		});

		const currentIds = new Set(this._tableItems.map((row) => row.id));
		for (const [id, rowContexts] of this.#rowContexts) {
			if (!currentIds.has(id)) {
				rowContexts.host.destroy();
				this.#rowContexts.delete(id);
			}
		}
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

	override disconnectedCallback() {
		super.disconnectedCallback();
		for (const [, rowContexts] of this.#rowContexts) {
			rowContexts.host.destroy();
		}
		this.#rowContexts.clear();
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
				.onRowRendered=${this.#onRowRendered}
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
