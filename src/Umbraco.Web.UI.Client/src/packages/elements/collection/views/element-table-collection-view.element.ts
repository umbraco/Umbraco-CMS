import { UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN } from '../../folder/workspace/constants.js';
import { UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_ELEMENT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbElementItemModel } from '../../types.js';
import type { UmbElementTreeItemModel } from '../../tree/types.js';
import { customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';

import './column-layouts/element-table-column-name.element.js';
import './column-layouts/element-table-column-state.element.js';

@customElement('umb-element-table-collection-view')
export class UmbElementTableCollectionViewElement extends UmbLitElement {
	@state()
	private _items?: Array<UmbElementTreeItemModel>;

	@state()
	private _selection: Array<string> = [];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext<any>;

	#tableConfig: UmbTableConfig = { allowSelection: true };

	#tableColumns: Array<UmbTableColumn> = [
		{ name: this.localize.term('general_name'), alias: 'name' },
		{ name: this.localize.term('content_publishStatus'), alias: 'state' },
		{ name: '', alias: 'entityActions', align: 'right' },
	];

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
			collectionContext?.setupView(this);
			this.#observeCollectionContext();
		});
	}

	#observeCollectionContext() {
		if (!this.#collectionContext) return;

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
				if (selection) {
					this._selection = selection;
				}
			},
			'_observeSelection',
		);
	}

	#createTableItems() {
		if (!this._items) return;

		this._tableItems = this._items.map((item) => {
			if (!item.unique) throw new Error('Item id is missing.');

			const editPath = item.isFolder
				? UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique })
				: UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique });

			const data = [
				{
					columnAlias: 'name',
					value: html`<umb-element-table-column-name
						.value=${{ item: item as UmbElementItemModel, editPath }}></umb-element-table-column-name>`,
				},
				{
					columnAlias: 'state',
					value: !item.isFolder
						? html`<umb-element-table-column-state
								.value=${item as UmbElementItemModel}></umb-element-table-column-state>`
						: nothing,
				},
				{
					columnAlias: 'entityActions',
					value: html`<umb-entity-actions-table-column-view .value=${item}></umb-entity-actions-table-column-view>`,
				},
			];

			return {
				id: item.unique,
				icon: item.isFolder && !item.icon ? 'icon-folder' : item.icon,
				entityType: item.isFolder ? UMB_ELEMENT_FOLDER_ENTITY_TYPE : UMB_ELEMENT_ENTITY_TYPE,
				data,
			};
		});
	}

	#onSelect(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#collectionContext?.selection.setSelection(selection);
	}

	#onDeselect(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#collectionContext?.selection.setSelection(selection);
	}

	override render() {
		return html`
			<umb-table
				.config=${this.#tableConfig}
				.columns=${this.#tableColumns}
				.items=${this._tableItems}
				.selection=${this._selection}
				@selected=${this.#onSelect}
				@deselected=${this.#onDeselect}>
			</umb-table>
		`;
	}
}

export { UmbElementTableCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-table-collection-view': UmbElementTableCollectionViewElement;
	}
}
