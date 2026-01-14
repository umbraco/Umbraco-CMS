import type { UmbElementTreeItemModel } from '../../tree/types.js';
import { UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN } from '../../folder/workspace/constants.js';
import { UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';

@customElement('umb-element-tree-item-table-collection-view')
export class UmbElementTreeItemTableCollectionViewElement extends UmbLitElement {
	@state()
	private _items?: Array<UmbElementTreeItemModel>;

	@state()
	private _selection: Array<string> = [];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext<any>;

	#routeBuilder?: UmbModalRouteBuilder;

	#tableConfig: UmbTableConfig = { allowSelection: true };

	#tableColumns: Array<UmbTableColumn> = [
		{
			name: this.localize.term('general_name'),
			alias: 'name',
		},
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
					this._selection = selection as string[];
				}
			},
			'_observeSelection',
		);

		this.observe(
			this.#collectionContext.workspacePathBuilder,
			(routeBuilder) => {
				this.#routeBuilder = routeBuilder;
				this.#createTableItems();
			},
			'_observeWorkspacePathBuilder',
		);
	}

	#createTableItems() {
		if (!this._items) return;
		const routeBuilder = this.#routeBuilder;
		if (!routeBuilder) return;

		this._tableItems = this._items.map((item) => {
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
						value: html`
							<uui-button
								compact
								href=${item.isFolder ? inlineEditPath : modalEditPath}
								label=${item.name}></uui-button>
						`,
					},
					{
						columnAlias: 'entityActions',
						value: html`<umb-entity-actions-table-column-view .value=${item}></umb-entity-actions-table-column-view>`,
					},
				],
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
