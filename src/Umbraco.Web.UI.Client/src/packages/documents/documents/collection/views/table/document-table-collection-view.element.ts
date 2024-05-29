import { getPropertyValueByAlias } from '../index.js';
import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type { UmbDocumentCollectionItemModel } from '../../types.js';
import type { UmbDocumentCollectionContext } from '../../document-collection.context.js';
import { UMB_DOCUMENT_COLLECTION_CONTEXT } from '../../document-collection.context-token.js';
import type { UmbCollectionColumnConfiguration } from '@umbraco-cms/backoffice/collection';
import { css, customElement, html, nothing, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/modal';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
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
import './column-layouts/document-table-column-state.element.js';

@customElement('umb-document-table-collection-view')
export class UmbDocumentTableCollectionViewElement extends UmbLitElement {
	@state()
	private _loading = false;

	@state()
	private _userDefinedProperties?: Array<UmbCollectionColumnConfiguration>;

	@state()
	private _items?: Array<UmbDocumentCollectionItemModel>;

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
	private _selection: Array<string> = [];

	#collectionContext?: UmbDocumentCollectionContext;

	#routeBuilder?: UmbModalRouteBuilder;

	constructor() {
		super();
		this.consumeContext(UMB_DOCUMENT_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
		});

		this.#registerModalRoute();
	}

	#registerModalRoute() {
		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(':entityType')
			.onSetup((params) => {
				return { data: { entityType: params.entityType, preset: {} } };
			})
			.onReject(() => {
				this.#collectionContext?.requestCollection();
			})
			.onSubmit(() => {
				this.#collectionContext?.requestCollection();
			})
			.observeRouteBuilder((routeBuilder) => {
				this.#routeBuilder = routeBuilder;

				// NOTE: Configuring the observations AFTER the route builder is ready,
				// otherwise there is a race condition and `#collectionContext.items` tends to win. [LK]
				this.#observeCollectionContext();
			});
	}

	#observeCollectionContext() {
		if (!this.#collectionContext) return;

		this.observe(this.#collectionContext.loading, (loading) => (this._loading = loading), '_observeLoading');

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
				this.#createTableItems(this._items);
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
					name: item.header,
					alias: item.alias,
					elementName: item.elementName,
					allowSorting: true,
				};
			});

			this._tableColumns = [...this.#systemColumns, ...userColumns];
		}
	}

	#createTableItems(items: Array<UmbDocumentCollectionItemModel>) {
		this._tableItems = items.map((item) => {
			if (!item.unique) throw new Error('Item id is missing.');

			const data =
				this._tableColumns?.map((column) => {
					const editPath = this.#routeBuilder
						? this.#routeBuilder({ entityType: item.entityType }) +
							UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateLocal({ unique: item.unique })
						: '';

					return {
						columnAlias: column.alias,
						value: column.elementName ? { item, editPath } : getPropertyValueByAlias(item, column.alias),
					};
				}) ?? [];

			return {
				id: item.unique,
				icon: item.icon,
				entityType: 'document',
				data: data,
			};
		});
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

	render() {
		return this._tableItems.length === 0 ? this.#renderEmpty() : this.#renderItems();
	}

	#renderEmpty() {
		if (this._tableItems.length > 0) return nothing;
		return html`
			<div class="container">
				${when(
					this._loading,
					() => html`<uui-loader></uui-loader>`,
					() => html`<p>${this.localize.term('content_listViewNoItems')}</p>`,
				)}
			</div>
		`;
	}

	#renderItems() {
		if (this._tableItems.length === 0) return nothing;
		return html`
			<umb-table
				.config=${this._tableConfig}
				.columns=${this._tableColumns}
				.items=${this._tableItems}
				.selection=${this._selection}
				@selected=${this.#handleSelect}
				@deselected=${this.#handleDeselect}
				@ordered=${this.#handleOrdering}></umb-table>
			${when(this._loading, () => html`<uui-loader-bar></uui-loader-bar>`)}
		`;
	}

	static styles = [
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
