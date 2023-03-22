import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { ifDefined } from 'lit/directives/if-defined.js';
import { when } from 'lit/directives/when.js';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';

// TODO: move to UI Library - entity actions should NOT be moved to UI Library but stay in an UmbTable element
export interface UmbTableItem {
	key: string;
	icon?: string;
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

/**
 *  @element umb-table
 *  @description - Element for displaying a table
 *  @fires {UmbTableSelectedEvent} selected - fires when a row is selected
 *  @fires {UmbTableDeselectedEvent} deselected - fires when a row is deselected
 *  @fires {UmbTableOrderedEvent} sort - fires when a column order is changed
 *  @extends LitElement
 */
@customElement('umb-table')
export class UmbTableElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
				overflow: auto;
				padding: var(--uui-size-space-4);
				padding-top: 0;
			}

			uui-table {
				box-shadow: var(--uui-shadow-depth-1);
			}

			uui-table-head {
				position: sticky;
				top: 0;
				background: white;
				z-index: 1;
				background-color: var(--uui-color-surface);
			}

			uui-table-row uui-checkbox {
				display: none;
			}

			uui-table-row[selectable]:focus uui-icon,
			uui-table-row[selectable]:focus-within uui-icon,
			uui-table-row[selectable]:hover uui-icon,
			uui-table-row[select-only] uui-icon {
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
		`,
	];

	/**
	 * Table Items
	 * @type {Array<UmbTableItem>}
	 * @memberof UmbTableElement
	 */
	@property({ type: Array, attribute: false })
	public items: Array<UmbTableItem> = [];

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

	@property({ type: String, attribute: false })
	public orderingDesc = false;

	@state()
	private _selectionMode = false;

	private _isSelected(key: string) {
		return this.selection.includes(key);
	}

	private _handleRowCheckboxChange(event: Event, item: UmbTableItem) {
		const checkboxElement = event.target as HTMLInputElement;
		checkboxElement.checked ? this._selectRow(item.key) : this._deselectRow(item.key);
	}

	private _handleAllRowsCheckboxChange(event: Event) {
		const checkboxElement = event.target as HTMLInputElement;
		checkboxElement.checked ? this._selectAllRows() : this._deselectAllRows();
	}

	private _handleOrderingChange(column: UmbTableColumn) {
		this.orderingDesc = this.orderingColumn === column.alias ? !this.orderingDesc : false;
		this.orderingColumn = column.alias;
		this.dispatchEvent(new UmbTableOrderedEvent());
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

	private _selectAllRows() {
		this.selection = this.items.map((item: UmbTableItem) => item.key);
		this._selectionMode = true;
		this.dispatchEvent(new UmbTableSelectedEvent());
	}

	private _deselectAllRows() {
		this.selection = [];
		this._selectionMode = false;
		this.dispatchEvent(new UmbTableDeselectedEvent());
	}

	render() {
		return html`<uui-table class="uui-text">
			<uui-table-column style="width: 60px;"></uui-table-column>
			<uui-table-head>
				${this._renderHeaderCheckboxCell()} ${this.columns.map((column) => this._renderHeaderCell(column))}
			</uui-table-head>
			${repeat(this.items, (item) => item.key, this._renderRow)}
		</uui-table>`;
	}

	private _renderHeaderCell(column: UmbTableColumn) {
		return html`
			<uui-table-head-cell style="--uui-table-cell-padding: 0 var(--uui-size-5)">
				${column.allowSorting ? html`${this._renderSortingUI(column)}` : column.name}
			</uui-table-head-cell>
		`;
	}

	private _renderSortingUI(column: UmbTableColumn) {
		return html`
			<button
				style="padding: var(--uui-size-4) var(--uui-size-5);"
				@click="${() => this._handleOrderingChange(column)}">
				${column.name}
				<uui-symbol-sort ?active=${this.orderingColumn === column.alias} ?descending=${this.orderingDesc}>
				</uui-symbol-sort>
			</button>
		`;
	}

	private _renderHeaderCheckboxCell() {
		if (this.config.hideIcon && !this.config.allowSelection) return;

		return html` <uui-table-head-cell style="--uui-table-cell-padding: 0">
			${when(
				this.config.allowSelection,
				() => html` <uui-checkbox
					label="Select All"
					style="padding: var(--uui-size-4) var(--uui-size-5);"
					@change="${this._handleAllRowsCheckboxChange}"
					?checked="${this.selection.length === this.items.length}">
				</uui-checkbox>`
			)}
		</uui-table-head-cell>`;
	}

	private _renderRow = (item: UmbTableItem) => {
		return html`<uui-table-row
			?selectable="${this.config.allowSelection}"
			?select-only=${this._selectionMode}
			?selected=${this._isSelected(item.key)}
			@selected=${() => this._selectRow(item.key)}
			@unselected=${() => this._deselectRow(item.key)}>
			${this._renderRowCheckboxCell(item)} ${this.columns.map((column) => this._renderRowCell(column, item))}
		</uui-table-row>`;
	};

	private _renderRowCheckboxCell(item: UmbTableItem) {
		if (this.config.hideIcon && !this.config.allowSelection) return;

		return html`<uui-table-cell>
			${when(!this.config.hideIcon, () => html`<uui-icon name=${ifDefined(item.icon)}></uui-icon>`)}
			${when(
				this.config.allowSelection,
				() => html` <uui-checkbox
					label="Select Row"
					@click=${(e: PointerEvent) => e.stopPropagation()}
					@change=${(event: Event) => this._handleRowCheckboxChange(event, item)}
					?checked="${this._isSelected(item.key)}">
				</uui-checkbox>`
			)}
		</uui-table-cell>`;
	}

	private _renderRowCell(column: UmbTableColumn, item: UmbTableItem) {
		return html`<uui-table-cell style="--uui-table-cell-padding: 0 var(--uui-size-5); width: ${column.width || 'auto'}"
			>${this._renderCellContent(column, item)}</uui-table-cell>
		</uui-table-cell>`;
	}

	private _renderCellContent(column: UmbTableColumn, item: UmbTableItem) {
		const value = item.data.find((data) => data.columnAlias === column.alias)?.value;

		if (column.elementName) {
			const element = document.createElement(column.elementName) as any; // TODO: add interface for UmbTableColumnLayoutElement
			element.column = column;
			element.item = item;
			element.value = value;
			return element;
		}

		return value;
	}
}

export default UmbTableElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-table': UmbTableElement;
	}
}
