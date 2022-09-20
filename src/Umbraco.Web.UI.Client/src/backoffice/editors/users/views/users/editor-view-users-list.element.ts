import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import { repeat } from 'lit/directives/repeat.js';

interface TableColumn {
	name: string;
	sort: Function;
}

interface TableItem {
	key: string;
	name: string;
	userGroup: string;
	lastLogin: string;
	status?: string;
}

@customElement('umb-editor-view-users-list')
export class UmbEditorViewUsersListElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			uui-table-row uui-checkbox {
				display: none;
			}
			uui-table-row:hover uui-icon,
			uui-table-row[select-only] uui-icon {
				display: none;
			}
			uui-table-row:hover uui-checkbox,
			uui-table-row[select-only] uui-checkbox {
				display: inline-block;
			}
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

	@property()
	public users: Array<TableItem> = [];

	@state()
	private _columns: Array<TableColumn> = [];

	@state()
	private _selectionMode = false;

	@state()
	private _selection: Array<string> = [];

	@state()
	private _sortingColumn: any = '';

	@state()
	private _sortingDesc = false;

	private _selectAllHandler(event: Event) {
		const checkboxElement = event.target as HTMLInputElement;
		this._selection = checkboxElement.checked ? this.users.map((item: TableItem) => item.key) : [];
		this._selectionMode = this._selection.length > 0;
	}

	private _selectHandler(event: Event, item: TableItem) {
		const checkboxElement = event.target as HTMLInputElement;
		this._selection = checkboxElement.checked
			? [...this._selection, item.key]
			: this._selection.filter((selectionKey) => selectionKey !== item.key);
		this._selectionMode = this._selection.length > 0;
	}
	private _selectRowHandler(item: TableItem) {
		this._selection = [...this._selection, item.key];
		this._selectionMode = this._selection.length > 0;
	}
	private _unselectRowHandler(item: TableItem) {
		this._selection = this._selection.filter((selectionKey) => selectionKey !== item.key);
		this._selectionMode = this._selection.length > 0;
	}

	private _sortingHandler(column: TableColumn) {
		this._sortingDesc = this._sortingColumn === column.name ? !this._sortingDesc : false;
		this._sortingColumn = column.name;
		this.users = column.sort(this.users, this._sortingDesc);
	}

	private _isSelected(key: string) {
		return this._selection.includes(key);
	}

	connectedCallback() {
		super.connectedCallback();

		this._columns = [
			{
				name: 'Name',
				sort: (items: Array<TableItem>, desc: boolean) => {
					return desc
						? [...items].sort((a, b) => b.name.localeCompare(a.name))
						: [...items].sort((a, b) => a.name.localeCompare(b.name));
				},
			},
			{
				name: 'User group',
				sort: (items: Array<TableItem>, desc: boolean) => {
					return desc
						? [...items].sort((a, b) => b.name.localeCompare(a.name))
						: [...items].sort((a, b) => a.name.localeCompare(b.name));
				},
			},
			{
				name: 'Last login',
				sort: (items: Array<TableItem>, desc: boolean) => {
					return desc
						? [...items].sort((a, b) => +new Date(b.lastLogin) - +new Date(a.lastLogin))
						: [...items].sort((a, b) => +new Date(a.lastLogin) - +new Date(b.lastLogin));
				},
			},
			{
				name: 'status',
				sort: (items: Array<TableItem>, desc: boolean) => {
					return desc
						? [...items].sort((a, b) =>
								b.status && a.status ? b.status.localeCompare(a.status) : (a.status ? 1 : 0) - (b.status ? 1 : 0)
						  )
						: [...items].sort((a, b) =>
								a.status && b.status ? a.status.localeCompare(b.status) : (b.status ? 1 : 0) - (a.status ? 1 : 0)
						  );
				},
			},
		];
	}

	renderHeaderCellTemplate(column: TableColumn) {
		return html`
			<uui-table-head-cell style="--uui-table-cell-padding: 0">
				<button style="padding: var(--uui-size-4) var(--uui-size-5);" @click="${() => this._sortingHandler(column)}">
					${column.name}
					<uui-symbol-sort ?active=${this._sortingColumn === column.name} ?descending=${this._sortingDesc}>
					</uui-symbol-sort>
				</button>
			</uui-table-head-cell>
		`;
	}

	protected renderRowTemplate = (item: TableItem) => {
		return html` <uui-table-row
			selectable
			?select-only=${this._selectionMode}
			?selected=${this._isSelected(item.key)}
			@selected=${() => this._selectRowHandler(item)}
			@unselected=${() => this._unselectRowHandler(item)}>
			<uui-table-cell>
				<div style="display: flex; align-items: center;">
					<uui-avatar name="${item.name}"></uui-avatar>
				</div>
			</uui-table-cell>
			<uui-table-cell>
				<div style="display: flex; align-items: center;">
					<a style="font-weight: bold;" href="http://">${item.name}</a>
				</div>
			</uui-table-cell>
			<uui-table-cell> ${item.userGroup} </uui-table-cell>
			<uui-table-cell>${item.lastLogin}</uui-table-cell>
			<uui-table-cell>${item.status}</uui-table-cell>
		</uui-table-row>`;
	};

	render() {
		return html`
			<uui-table class="uui-text">
				<uui-table-column style="width: 60px;"></uui-table-column>
				<uui-table-head>
					<uui-table-head-cell style="--uui-table-cell-padding: 0">
						<uui-checkbox
							style="padding: var(--uui-size-4) var(--uui-size-5);"
							@change="${this._selectAllHandler}"
							?checked="${this._selection.length === this.users.length}"></uui-checkbox>
					</uui-table-head-cell>
					${this._columns.map((column) => this.renderHeaderCellTemplate(column))}
				</uui-table-head>
				${repeat(this.users, (item) => item.key, this.renderRowTemplate)}
			</uui-table>
		`;
	}
}

export default UmbEditorViewUsersListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-list': UmbEditorViewUsersListElement;
	}
}
