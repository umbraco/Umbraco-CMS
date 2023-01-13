import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import type { UmbCollectionContext } from '../collection.context';
import type { MediaDetails } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';
import {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSelectedEvent,
} from '../../components/table';

@customElement('umb-collection-view-media-table')
export class UmbCollectionViewMediaTableElement extends UmbLitElement {
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

			umb-table {
				padding: 0; /* To fix the embedded padding in the table component. */
			}
		`,
	];

	@state()
	private _mediaItems?: Array<MediaDetails>;

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
		{
			name: 'Last edited',
			alias: 'mediaLastEdited',
		},
		{
			name: 'Created by',
			alias: 'mediaCreatedBy',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	@state()
	private _selection: Array<string> = [];

	private _collectionContext?: UmbCollectionContext<MediaDetails>;

	constructor() {
		super();
		this.consumeContext('umbCollectionContext', (instance) => {
			this._collectionContext = instance;
			this._observeCollectionContext();
		});
	}

	private _observeCollectionContext() {
		if (!this._collectionContext) return;

		this.observe(this._collectionContext.data, (nodes) => {
			this._mediaItems = nodes;
			this._createTableItems(this._mediaItems);
		});

		this.observe(this._collectionContext.selection, (selection) => {
			this._selection = selection;
		});
	}

	private _createTableItems(mediaItems: Array<MediaDetails>) {
		// TODO: I guess the type error below will go away when we get an entity based MediaDetails model instead of tree based.
		// @ts-ignore // TODO: Remove ts-ignore when Media type gets fixed.
		this._tableItems = mediaItems.map((item) => {
			return {
				key: item.key,
				icon: item.icon,
				data: [
					{
						columnAlias: 'mediaName',
						value: item.name || 'Untitled',
					},
					{
						columnAlias: 'mediaLastEdited',
						value: 'not implemented',
					},
					{
						columnAlias: 'mediaCreatedBy',
						value: 'not implemented',
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-media-table': UmbCollectionViewMediaTableElement;
	}
}
