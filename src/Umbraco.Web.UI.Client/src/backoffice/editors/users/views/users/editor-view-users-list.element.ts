import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
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

	@state()
	private _columns: Array<TableColumn> = [];

	@state()
	private _items: Array<TableItem> = [];

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
		this._selection = checkboxElement.checked ? this._items.map((item: TableItem) => item.key) : [];
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
		this._items = column.sort(this._items, this._sortingDesc);
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

		this._items = [
			{
				key: 'a9b18a00-58f2-420e-bf60-48d33ab156db',
				name: 'Cecílie Bryon',
				userGroup: 'Translators',
				lastLogin: 'Fri, 23 April 2021',
				status: 'Invited',
			},
			{
				key: '3179d0b2-eec2-4045-b86a-149e13b93e14',
				name: 'Kathleen G. Smith',
				userGroup: 'Editors',
				lastLogin: 'Tue, 6 June 2021', // random date
				status: 'Invited',
			},
			{
				key: '1b1c9733-b845-4d9a-9ed2-b2f46c05fd72',
				name: 'Adrian Andresen',
				userGroup: 'Administrators',
				lastLogin: 'Mon, 15 November 2021',
			},
			{
				key: 'b75af81a-b994-4e65-9330-b66c336d0207',
				name: 'Lorenza Trentino',
				userGroup: 'Editors',
				lastLogin: 'Fri, 13 April 2022',
			},
			{
				key: 'b75af81a-b994-4e65-9330-b66c336d0202',
				name: 'John Doe',
				userGroup: 'Translators',
				lastLogin: 'Tue, 11 December 2021',
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
			<div style="margin-bottom: 20px;">Selected ${this._selection.length} of ${this._items.length}</div>
			<uui-table class="uui-text">
				<uui-table-column style="width: 60px;"></uui-table-column>
				<uui-table-head>
					<uui-table-head-cell style="--uui-table-cell-padding: 0">
						<uui-checkbox
							style="padding: var(--uui-size-4) var(--uui-size-5);"
							@change="${this._selectAllHandler}"
							?checked="${this._selection.length === this._items.length}"></uui-checkbox>
					</uui-table-head-cell>
					${this._columns.map((column) => this.renderHeaderCellTemplate(column))}
				</uui-table-head>
				${repeat(this._items, (item) => item.key, this.renderRowTemplate)}
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
