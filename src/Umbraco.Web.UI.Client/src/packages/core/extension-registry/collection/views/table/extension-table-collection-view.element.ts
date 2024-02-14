import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

import '../extension-table-action-column-layout.element.js';

@customElement('umb-extension-table-collection-view')
export class UmbExtensionTableCollectionViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
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
			alias: 'extensionAction',
			elementName: 'umb-extension-table-action-column-layout',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext<ManifestBase>;

	constructor() {
		super();

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	#observeCollectionItems() {
		if (!this.#collectionContext) return;
		this.observe(this.#collectionContext.items, (items) => this.#createTableItems(items), 'umbCollectionItemsObserver');
	}

	#createTableItems(extensions: Array<ManifestBase>) {
		this._tableItems = extensions.map((extension) => {
			return {
				id: extension.alias,
				data: [
					{
						columnAlias: 'extensionType',
						value: extension.type,
					},
					{
						columnAlias: 'extensionName',
						value: extension.name,
					},
					{
						columnAlias: 'extensionAlias',
						value: extension.alias,
					},
					{
						columnAlias: 'extensionWeight',
						value: extension.weight,
					},
					{
						columnAlias: 'extensionAction',
						value: extension,
					},
				],
			};
		});
	}

	render() {
		return html`
			<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
		`;
	}

	static styles = [
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
