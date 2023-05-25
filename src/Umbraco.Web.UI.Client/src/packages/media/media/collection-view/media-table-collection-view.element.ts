import type { MediaDetails } from '../index.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html , customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';
import { UmbCollectionContext, UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-media-table-collection-view')
export class UmbMediaTableCollectionViewElement extends UmbLitElement {
	@state()
	private _mediaItems?: Array<EntityTreeItemResponseModel>;

	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: true,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Name',
			alias: 'mediaName',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	@state()
	private _selection: Array<string> = [];

	private _collectionContext?: UmbCollectionContext<MediaDetails, any>;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this._collectionContext = instance;
			this._observeCollectionContext();
		});
	}

	private _observeCollectionContext() {
		if (!this._collectionContext) return;

		this.observe(this._collectionContext.items, (nodes) => {
			this._mediaItems = nodes;
			this._createTableItems(this._mediaItems);
		});

		this.observe(this._collectionContext.selection, (selection) => {
			this._selection = selection;
		});
	}

	private _createTableItems(mediaItems: Array<any>) {
		// TODO: this should use the MediaDetails type, but for now that results in type errors.
		// TODO: I guess the type error will go away when we get an entity based MediaDetails model instead of tree based.
		this._tableItems = mediaItems.map((item) => {
			return {
				id: item.id,
				icon: item.icon,
				data: [
					{
						columnAlias: 'mediaName',
						value: item.name || 'Untitled',
					},
				],
			};
		});
	}

	private _handleSelect(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this._collectionContext?.setSelection(selection);
	}

	private _handleDeselect(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this._collectionContext?.setSelection(selection);
	}

	private _handleOrdering(event: UmbTableOrderedEvent) {
		const table = event.target as UmbTableElement;
		const orderingColumn = table.orderingColumn;
		const orderingDesc = table.orderingDesc;
		console.log(`fetch media items, order column: ${orderingColumn}, desc: ${orderingDesc}`);
	}

	render() {
		return html`
			<umb-table
				.config=${this._tableConfig}
				.columns=${this._tableColumns}
				.items=${this._tableItems}
				.selection=${this._selection}
				@selected="${this._handleSelect}"
				@deselected="${this._handleDeselect}"
				@ordered="${this._handleOrdering}"></umb-table>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				box-sizing: border-box;
				height: 100%;
				width: 100%;
				padding: var(--uui-size-space-3) var(--uui-size-space-6);
			}

			/* TODO: Should we have embedded padding in the table component? */
			umb-table {
				padding: 0; /* To fix the embedded padding in the table component. */
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
