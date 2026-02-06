import type { UmbExtensionCollectionFilterModel, UmbExtensionCollectionItemModel } from '../../types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-extension-table-collection-view')
export class UmbExtensionTableCollectionViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
		hideIcon: true,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Type',
			alias: 'extensionType',
		},
		{
			name: 'Name',
			alias: 'extensionName',
		},
		{
			name: 'Alias',
			alias: 'extensionAlias',
		},
		{
			name: 'Weight',
			alias: 'extensionWeight',
		},
		{
			name: '',
			alias: 'entityActions',
			align: 'right',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext<UmbExtensionCollectionItemModel, UmbExtensionCollectionFilterModel>;

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

	#createTableItems(extensions: Array<UmbExtensionCollectionItemModel>) {
		this._tableItems = extensions.map((extension) => {
			return {
				id: extension.unique,
				data: [
					{
						columnAlias: 'extensionType',
						value: extension.manifest.type,
					},
					{
						columnAlias: 'extensionName',
						value: extension.manifest.name,
					},
					{
						columnAlias: 'extensionAlias',
						value: extension.manifest.alias,
					},
					{
						columnAlias: 'extensionWeight',
						value: extension.manifest.weight,
					},
					{
						columnAlias: 'entityActions',
						value: html`<umb-entity-actions-table-column-view
							.value=${{
								entityType: extension.entityType,
								unique: extension.unique,
								name: extension.manifest.name,
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

export default UmbExtensionTableCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-table-collection-view': UmbExtensionTableCollectionViewElement;
	}
}
