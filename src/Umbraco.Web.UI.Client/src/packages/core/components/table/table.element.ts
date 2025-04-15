import type { UmbUfmRenderElement } from '../../../ufm/components/ufm-render/index.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	property,
	repeat,
	state,
	when,
	LitElement,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

export interface UmbTableItem {
	id: string;
	icon?: string | null;
	entityType?: string;
	data: Array<UmbTableItemData>;
}

export interface UmbTableItemData {
	columnAlias: string;
	value: any;
}

export interface UmbTableColumn {
	name: string;
	alias: string;
	elementName?: string;
	width?: string;
	allowSorting?: boolean;
	align?: 'left' | 'center' | 'right';
	labelTemplate?: string;
}

export interface UmbTableColumnLayoutElement extends HTMLElement {
	column: UmbTableColumn;
	item: UmbTableItem;
	value: any;
}

export interface UmbTableConfig {
	allowSelection: boolean;
	hideIcon?: boolean;
}

export class UmbTableSelectedEvent extends Event {
	public constructor() {
		super('selected', { bubbles: true, composed: true });
	}
}

export class UmbTableDeselectedEvent extends Event {
	public constructor() {
		super('deselected', { bubbles: true, composed: true });
	}
}

export class UmbTableOrderedEvent extends Event {
	public constructor() {
		super('ordered', { bubbles: true, composed: true });
	}
}

export class UmbTableSortedEvent extends Event {
	#itemId: string;

	public constructor({ itemId }: { itemId: string }) {
		super('sorted', { bubbles: true, composed: true });
		this.#itemId = itemId;
	}

	public getItemId() {
		return this.#itemId;
	}
}

/**
 *  @element umb-table
 *  @description - Element for displaying a table
 *  @fires {UmbTableSelectedEvent} selected - fires when a row is selected
 *  @fires {UmbTableDeselectedEvent} deselected - fires when a row is deselected
 *  @fires {UmbTableOrderedEvent} sort - fires when a column order is changed
 *  @augments LitElement
 */
@customElement('umb-table')
export class UmbTableElement extends UmbLitElement {
	/**
	 * Table Items
	 * @type {Array<UmbTableItem>}
	 * @memberof UmbTableElement
	 */
	@property({ type: Array, attribute: false })
	private _items: Array<UmbTableItem> = [];
	public get items(): Array<UmbTableItem> {
		return this._items;
	}
	public set items(value: Array<UmbTableItem>) {
		this._items = value;
		this.#sorter.setModel(value);
	}

	/**
	 * @description Table Columns
	 * @type {Array<UmbTableColumn>}
	 * @memberof UmbTableElement
	 */
	@property({ type: Array, attribute: false })
	public columns: Array<UmbTableColumn> = [];

	/**
	 * @description Table Config
	 * @type {UmbTableConfig}
	 * @memberof UmbTableElement
	 */
	@property({ type: Object, attribute: false })
	public config: UmbTableConfig = {
		allowSelection: false,
		hideIcon: false,
	};

	/**
	 * @description Table Selection
	 * @type {Array<string>}
	 * @memberof UmbTableElement
	 */
	@property({ type: Array, attribute: false })
	public selection: Array<string> = [];

	@property({ type: String, attribute: false })
	public orderingColumn = '';

	@property({ type: Boolean, attribute: false })
	public orderingDesc = false;

	@property({ type: Boolean })
	private _sortable = false;
	public get sortable() {
		return this._sortable;
	}
	public set sortable(value) {
		this._sortable = value;
		if (value) {
			this.#sorter.enable();
		} else {
			this.#sorter.disable();
		}
	}

	@state()
	private _selectionMode = false;

	#sorter = new UmbSorterController<UmbTableItem>(this, {
		getUniqueOfElement: (element) => {
			return element.dataset.sortableId;
		},
		getUniqueOfModel: (item) => {
			return item.id;
		},
		identifier: 'Umb.SorterIdentifier.UmbTable',
		itemSelector: 'uui-table-row',
		containerSelector: 'uui-table',
		onChange: ({ model }) => {
			const oldValue = this.items;
			this.items = model;
			this.requestUpdate('items', oldValue);
		},
		onEnd: ({ item }) => {
			this.dispatchEvent(new UmbTableSortedEvent({ itemId: item.id }));
		},
	});

	constructor() {
		super();
		this.#sorter.disable();
	}

	private _isSelected(key: string) {
		return this.selection.includes(key);
	}

	private _handleRowCheckboxChange(event: Event, item: UmbTableItem) {
		const checkboxElement = event.target as HTMLInputElement;
		if (checkboxElement.checked) {
			this._selectRow(item.id);
		} else {
			this._deselectRow(item.id);
		}
	}

	private _handleAllRowsCheckboxChange(event: Event) {
		const checkboxElement = event.target as HTMLInputElement;
		if (checkboxElement.checked) {
			this._selectAllRows();
		} else {
			this._deselectAllRows();
		}
	}

	private _handleOrderingChange(column: UmbTableColumn) {
		this.orderingDesc = this.orderingColumn === column.alias ? !this.orderingDesc : false;
		this.orderingColumn = column.alias;
		this.dispatchEvent(new UmbTableOrderedEvent());
	}

	private _selectAllRows() {
		this.selection = this.items.map((item: UmbTableItem) => item.id);
		this._selectionMode = true;
		this.dispatchEvent(new UmbTableSelectedEvent());
	}

	private _deselectAllRows() {
		this.selection = [];
		this._selectionMode = false;
		this.dispatchEvent(new UmbTableDeselectedEvent());
	}

	private _selectRow(key: string) {
		this.selection = [...this.selection, key];
		this._selectionMode = this.selection.length > 0;
		this.dispatchEvent(new UmbTableSelectedEvent());
	}

	private _deselectRow(key: string) {
		this.selection = this.selection.filter((selectionKey) => selectionKey !== key);
		this._selectionMode = this.selection.length > 0;
		this.dispatchEvent(new UmbTableDeselectedEvent());
	}

	override render() {
		return html`
			<uui-table class="uui-text">
				<uui-table-column
					.style=${when(
						!(this.config.allowSelection === false && this.config.hideIcon === true),
						() => 'width: 60px',
					)}></uui-table-column>
				<uui-table-head>
					${this._renderHeaderCheckboxCell()} ${this.columns.map((column) => this._renderHeaderCell(column))}
				</uui-table-head>
				${repeat(this.items, (item) => item.id, this._renderRow)}
			</uui-table>
		`;
	}

	private _renderHeaderCell(column: UmbTableColumn) {
		return html`
			<uui-table-head-cell style="--uui-table-cell-padding: 0 var(--uui-size-5)">
				${column.allowSorting
					? html`${this._renderSortingUI(column)}`
					: html`<span style="text-align:${column.align ?? 'left'};">${column.name}</span>`}
			</uui-table-head-cell>
		`;
	}

	private _renderSortingUI(column: UmbTableColumn) {
		return html`
			<button
				style="padding: var(--uui-size-5) var(--uui-size-1);"
				@click="${() => this._handleOrderingChange(column)}">
				<span style="text-align:${column.align ?? 'left'};">${column.name}</span>
				<uui-symbol-sort ?active=${this.orderingColumn === column.alias} ?descending=${this.orderingDesc}>
				</uui-symbol-sort>
			</button>
		`;
	}

	private _renderHeaderCheckboxCell() {
		if (this.config.hideIcon && !this.config.allowSelection) return;

		return html`
			<uui-table-head-cell style="--uui-table-cell-padding: 0; text-align: center;">
				${when(
					this.config.allowSelection,
					() =>
						html` <uui-checkbox
							label="Select All"
							style="padding: var(--uui-size-4) var(--uui-size-5);"
							@change="${this._handleAllRowsCheckboxChange}"
							?checked="${this.selection.length === this.items.length}">
						</uui-checkbox>`,
				)}
			</uui-table-head-cell>
		`;
	}

	private _renderRow = (item: UmbTableItem) => {
		return html`
			<uui-table-row
				data-sortable-id=${item.id}
				?selectable="${this.config.allowSelection}"
				?select-only=${this._selectionMode}
				?selected=${this._isSelected(item.id)}
				@selected=${() => this._selectRow(item.id)}
				@deselected=${() => this._deselectRow(item.id)}>
				${this._renderRowCheckboxCell(item)} ${this.columns.map((column) => this._renderRowCell(column, item))}
			</uui-table-row>
		`;
	};

	private _renderRowCheckboxCell(item: UmbTableItem) {
		if (this.config.hideIcon && !this.config.allowSelection) return;

		return html`
			<uui-table-cell style="text-align: center;">
				${when(!this.config.hideIcon, () => html`<umb-icon name="${ifDefined(item.icon ?? undefined)}"></umb-icon>`)}
				${when(
					this.config.allowSelection,
					() => html`
						<uui-checkbox
							label="Select Row"
							@click=${(e: PointerEvent) => e.stopPropagation()}
							@change=${(event: Event) => this._handleRowCheckboxChange(event, item)}
							?checked="${this._isSelected(item.id)}">
						</uui-checkbox>
					`,
				)}
			</uui-table-cell>
		`;
	}

	private _renderRowCell(column: UmbTableColumn, item: UmbTableItem) {
		return html`
			<uui-table-cell
				style="--uui-table-cell-padding: 0 var(--uui-size-5); text-align:${column.align ?? 'left'}; width: ${column.width || 'auto'};">
					${this._renderCellContent(column, item)}
			</uui-table-cell>
		</uui-table-cell>
		`;
	}

	private _renderCellContent(column: UmbTableColumn, item: UmbTableItem) {
		const value = item.data.find((data) => data.columnAlias === column.alias)?.value;

		if (column.elementName) {
			const element = document.createElement(column.elementName) as UmbTableColumnLayoutElement;
			element.column = column;
			element.item = item;
			element.value = value;
			return element;
		}

		if (column.labelTemplate) {
			import('@umbraco-cms/backoffice/ufm');
			const element = document.createElement('umb-ufm-render') as UmbUfmRenderElement;
			element.inline = true;
			element.markdown = column.labelTemplate;
			element.value = { value };
			return element;
		}

		return value;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				height: fit-content;
			}

			uui-table {
				box-shadow: var(--uui-shadow-depth-1);
			}

			uui-table-head {
				position: sticky;
				top: 0;
				z-index: 1;
				background-color: var(--uui-color-surface, #fff);
			}

			uui-table-row uui-checkbox {
				display: none;
			}

			uui-table-row[selectable]:focus umb-icon,
			uui-table-row[selectable]:focus-within umb-icon,
			uui-table-row[selectable]:hover umb-icon,
			uui-table-row[select-only] umb-icon {
				display: none;
			}

			uui-table-row[selectable]:focus uui-checkbox,
			uui-table-row[selectable]:focus-within uui-checkbox,
			uui-table-row[selectable]:hover uui-checkbox,
			uui-table-row[select-only] uui-checkbox {
				display: inline-block;
			}

			uui-table-head-cell:focus,
			uui-table-head-cell:focus-within,
			uui-table-head-cell:hover {
				--uui-symbol-sort-hover: 1;
			}

			uui-table-head-cell button {
				padding: 0;
				background-color: transparent;
				color: inherit;
				border: none;
				cursor: pointer;
				font-weight: inherit;
				font-size: inherit;
				display: inline-flex;
				align-items: center;
				justify-content: space-between;
				width: 100%;
			}

			uui-table-head-cell button > span {
				flex: 1 0 auto;
			}

			uui-table-cell umb-icon {
				vertical-align: top;
			}
		`,
	];
}

export default UmbTableElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-table': UmbTableElement;
	}
}
