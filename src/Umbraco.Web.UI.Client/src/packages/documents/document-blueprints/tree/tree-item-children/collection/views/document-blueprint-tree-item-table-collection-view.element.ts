import { UMB_EDIT_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../../../../paths.js';
import { UMB_EDIT_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_PATH_PATTERN } from '../../../folder/workspace/paths.js';
import type { UmbDocumentBlueprintTreeItemModel } from '../../../types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-blueprint-tree-item-table-collection-view')
export class UmbDocumentBlueprintTreeItemTableCollectionViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Name',
			alias: 'name',
		},
		{
			name: '',
			alias: 'entityActions',
			align: 'right',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext<any>;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	#observeCollectionItems() {
		if (!this.#collectionContext) return;
		this.observe(this.#collectionContext.items, (items) => this.#createTableItems(items), 'umbCollectionItemsObserver');
	}

	#createTableItems(items: Array<UmbDocumentBlueprintTreeItemModel>) {
		this._tableItems = items.map((item) => {
			const editPath = item.isFolder
				? UMB_EDIT_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique })
				: UMB_EDIT_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique });

			return {
				id: item.unique,
				icon: item.isFolder && !item.icon ? 'icon-folder' : item.icon,
				data: [
					{
						columnAlias: 'name',
						value: html`<uui-button compact href=${editPath} label=${item.name}></uui-button>`,
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

export { UmbDocumentBlueprintTreeItemTableCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-tree-item-table-collection-view': UmbDocumentBlueprintTreeItemTableCollectionViewElement;
	}
}
