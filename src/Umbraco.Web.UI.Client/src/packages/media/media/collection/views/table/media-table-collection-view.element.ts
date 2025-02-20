import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from '../../types.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from '../../media-collection.context-token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbDefaultCollectionContext, UmbCollectionColumnConfiguration } from '@umbraco-cms/backoffice/collection';
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
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';

@customElement('umb-media-table-collection-view')
export class UmbMediaTableCollectionViewElement extends UmbLitElement {
	@state()
	private _userDefinedProperties?: Array<UmbCollectionColumnConfiguration>;

	@state()
	private _workspacePathBuilder?: UmbModalRouteBuilder;

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
			alias: 'name',
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
		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
			this.#observeCollectionContext();
			collectionContext.setupView(this);
			this.observe(
				collectionContext.workspacePathBuilder,
				(builder) => {
					this._workspacePathBuilder = builder;
					this.#createTableItems();
				},
				'observePath',
			);
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
			'_observeUserDefinedProperties',
		);

		this.observe(
			this.#collectionContext.items,
			(items) => {
				this._items = items;
				this.#createTableItems();
			},
			'_observeItems',
		);

		this.observe(
			this.#collectionContext.selection.selection,
			(selection) => {
				this._selection = selection as string[];
			},
			'_observeSelection',
		);
	}

	#createTableHeadings() {
		if (this._userDefinedProperties && this._userDefinedProperties.length > 0) {
			const userColumns: Array<UmbTableColumn> = this._userDefinedProperties.map((item) => {
				return {
					name: this.localize.string(item.header),
					alias: item.alias,
					elementName: item.elementName,
					allowSorting: true,
				};
			});

			this._tableColumns = [
				...this.#systemColumns,
				...userColumns,
				{ name: '', alias: 'entityActions', align: 'right' },
			];
		} else {
			this._tableColumns = [...this.#systemColumns, { name: '', alias: 'entityActions', align: 'right' }];
		}
	}

	#createTableItems() {
		this._tableItems = [];

		if (this._items === undefined) return;
		if (this._workspacePathBuilder === undefined) return;

		if (this._tableColumns.length === 0) {
			this.#createTableHeadings();
		}

		this._tableItems = this._items.map((item) => {
			if (!item.unique) throw new Error('Item id is missing.');

			const data =
				this._tableColumns?.map((column) => {
					if (column.alias === 'entityActions') {
						return {
							columnAlias: 'entityActions',
							value: html`<umb-entity-actions-table-column-view
								.value=${{
									entityType: item.entityType,
									unique: item.unique,
								}}></umb-entity-actions-table-column-view>`,
						};
					}

					const editPath =
						item.unique && this._workspacePathBuilder
							? this._workspacePathBuilder({ entityType: item.entityType }) +
								UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateLocal({
									unique: item.unique,
								})
							: '';

					return {
						columnAlias: column.alias,
						value: column.elementName ? { item, editPath } : this.#getPropertyValueByAlias(item, column.alias),
					};
				}) ?? [];

			return {
				id: item.unique,
				icon: item.icon,
				entityType: 'media',
				data: data,
			};
		});
	}

	#getPropertyValueByAlias(item: UmbMediaCollectionItemModel, alias: string) {
		switch (alias) {
			case 'contentTypeAlias':
				return item.contentTypeAlias;
			case 'createDate':
				return item.createDate.toLocaleString();
			case 'name':
				return item.name;
			case 'creator':
			case 'owner':
				return item.creator;
			case 'sortOrder':
				return item.sortOrder;
			case 'updateDate':
				return item.updateDate.toLocaleString();
			case 'updater':
				return item.updater;
			default:
				return item.values?.find((value) => value.alias === alias)?.value ?? '';
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

	override render() {
		return html`
			<umb-table
				.config=${this._tableConfig}
				.columns=${this._tableColumns}
				.items=${this._tableItems}
				.selection=${this._selection}
				@selected=${this.#handleSelect}
				@deselected=${this.#handleDeselect}
				@ordered=${this.#handleOrdering}></umb-table>
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
