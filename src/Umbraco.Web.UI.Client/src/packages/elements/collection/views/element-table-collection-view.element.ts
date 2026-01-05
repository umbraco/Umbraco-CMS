import type { UmbElementTreeItemModel } from '../../tree/types.js';
import { UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN } from '../../tree/folder/workspace/constants.js';
import { UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-element-tree-item-table-collection-view')
export class UmbElementTreeItemTableCollectionViewElement extends UmbLitElement {
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
	#routeBuilder?: UmbModalRouteBuilder;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});

		this.#registerModalRoute();
	}

	#registerModalRoute() {
		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(':entityType')
			.onSetup((params) => {
				return { data: { entityType: params.entityType, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this.#routeBuilder = routeBuilder;

				// NOTE: Configuring the observations AFTER the route builder is ready,
				// otherwise there is a race condition and `#collectionContext.items` tends to win. [LK]
				this.#observeCollectionItems();
			});
	}

	#observeCollectionItems() {
		if (!this.#collectionContext) return;
		this.observe(this.#collectionContext.items, (items) => this.#createTableItems(items), 'umbCollectionItemsObserver');
	}

	#createTableItems(items: Array<UmbElementTreeItemModel>) {
		const routeBuilder = this.#routeBuilder;
		if (!routeBuilder) throw new Error('Route builder not ready');

		this._tableItems = items.map((item) => {
			const modalEditPath =
				routeBuilder({ entityType: item.entityType }) +
				UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN.generateLocal({ unique: item.unique });
			const inlineEditPath = UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN.generateAbsolute({
				unique: item.unique,
			});

			return {
				id: item.unique,
				icon: item.isFolder && !item.icon ? 'icon-folder' : item.icon,
				data: [
					{
						columnAlias: 'name',
						value: html`<uui-button
							href=${item.isFolder ? inlineEditPath : modalEditPath}
							label=${item.name}></uui-button>`,
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

export { UmbElementTreeItemTableCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-tree-item-table-collection-view': UmbElementTreeItemTableCollectionViewElement;
	}
}
