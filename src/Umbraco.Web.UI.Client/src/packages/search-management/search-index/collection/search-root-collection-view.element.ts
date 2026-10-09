import type { UmbSearchIndex } from '../types.js';
import { UMB_EDIT_SEARCH_INDEX_WORKSPACE_PATH_PATTERN } from '../workspace/paths.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UMB_COLLECTION_CONTEXT, type UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-search-root-collection-view')
export default class UmbSearchRootCollectionViewElement extends UmbLitElement {
	@state()
	private _tableItems: Array<UmbTableItem> = [];

	private readonly _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	private readonly _tableColumns: Array<UmbTableColumn> = [
		{
			name: this.localize.term('searchManagement_tableColumnAlias'),
			alias: 'indexAlias',
		},
		{
			name: this.localize.term('searchManagement_tableColumnHealthStatus'),
			alias: 'healthStatus',
		},
		{
			name: this.localize.term('searchManagement_tableColumnDocumentCount'),
			alias: 'documentCount',
		},
		{
			name: '',
			alias: 'entityActions',
			align: 'right',
		},
	];

	#collectionContext?: UmbDefaultCollectionContext<UmbSearchIndex>;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	override render() {
		return html`
			<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
		`;
	}

	#observeCollectionItems() {
		this.observe(
			this.#collectionContext?.items,
			(items) => {
				// Make sure we are connected to the DOM, otherwise we might update state when not needed
				// or when changing to another workspace with similar context.
				if (!this.isConnected) return;

				this.#createTable(items ?? []);
			},
			'_itemsObserver',
		);
	}

	#createTable(items: UmbSearchIndex[]) {
		this._tableItems = items?.map((item) => {
			const editHref = UMB_EDIT_SEARCH_INDEX_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique });
			return {
				id: item.unique,
				icon: 'icon-search',
				data: [
					{
						columnAlias: 'indexAlias',
						value: html`<a href=${editHref}>${item.unique}</a>`,
					},
					{
						columnAlias: 'healthStatus',
						value: when(
							item.state === 'loading',
							() => html`<uui-loader-bar></uui-loader-bar>`,
							() => this.localize.term('searchManagement_healthStatus', item.healthStatus),
						),
					},
					{
						columnAlias: 'documentCount',
						value: this.localize.term('searchManagement_documentCount', this.localize.number(item.documentCount)),
					},
					{
						columnAlias: 'entityActions',
						value: html`<umb-entity-actions-table-column-view
							.value=${{
								entityType: item.entityType,
								unique: item.unique,
								name: item.unique,
							}}></umb-entity-actions-table-column-view>`,
					},
				],
			};
		});
	}

	static override readonly styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-search-root-collection-view': UmbSearchRootCollectionViewElement;
	}
}
