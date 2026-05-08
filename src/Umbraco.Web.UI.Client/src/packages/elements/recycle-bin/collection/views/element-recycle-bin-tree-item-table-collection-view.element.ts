import type { UmbElementRecycleBinTreeItemModel } from '../../tree/types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';

import './trashed-element-name-table-column.element.js';

@customElement('umb-element-recycle-bin-tree-item-table-collection-view')
export class UmbElementRecycleBinTreeItemTableCollectionViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: this.localize.term('general_name'),
			alias: 'name',
			elementName: 'umb-trashed-element-name-table-column',
		},
		{
			name: '',
			alias: 'entityActions',
			align: 'right',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext;
	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	constructor() {
		super();
		this.#isTrashedContext.setIsTrashed(true);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	#observeCollectionItems() {
		if (!this.#collectionContext) return;
		this.observe(this.#collectionContext.items, (items) => this.#createTableItems(items), 'umbCollectionItemsObserver');
	}

	#createTableItems(items: Array<UmbElementRecycleBinTreeItemModel>) {
		this._tableItems = items.map((item) => {
			return {
				id: item.unique,
				icon: item.documentType.icon,
				data: [
					{
						columnAlias: 'name',
						value: item,
					},
					{
						columnAlias: 'entityActions',
						value: html`<umb-entity-actions-table-column-view
							.value=${{
								entityType: item.entityType,
								unique: item.unique,
								name: item.name,
							}}></umb-entity-actions-table-column-view>`,
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

export { UmbElementRecycleBinTreeItemTableCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-recycle-bin-tree-item-table-collection-view': UmbElementRecycleBinTreeItemTableCollectionViewElement;
	}
}
