import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import { repeat } from 'lit/directives/repeat.js';
import UmbEditorViewUsersElement, { UserItem } from './editor-view-users.element';
import { Subscription } from 'rxjs';

interface TableColumn {
	name: string;
	sort: (items: Array<UserItem>, desc: boolean) => Array<UserItem>;
}

@customElement('umb-editor-view-users-list')
export class UmbEditorViewUsersListElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			uui-table {
				box-shadow: var(--uui-shadow-depth-1, 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24));
			}
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
	private _selectionMode = false;

	@state()
	private _selection: Array<string> = [];

	@state()
	private _sortingColumn: any = '';

	@state()
	private _sortingDesc = false;

	@state()
	private _users: Array<UserItem> = [];

	protected _usersSubscription?: Subscription;
	protected _usersContext?: UmbEditorViewUsersElement;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUsersContext', (usersContext: UmbEditorViewUsersElement) => {
			this._usersContext = usersContext;

			this._usersSubscription?.unsubscribe();
			this._usersSubscription = this._usersContext?.users.subscribe((users: Array<UserItem>) => {
				this._users = users;
			});
		});

		this._columns = [
			{
				name: 'Name',
				sort: (items: Array<UserItem>, desc: boolean) => {
					return desc
						? [...items].sort((a, b) => b.name.localeCompare(a.name))
						: [...items].sort((a, b) => a.name.localeCompare(b.name));
				},
			},
			{
				name: 'User group',
				sort: (items: Array<UserItem>, desc: boolean) => {
					return desc
						? [...items].sort((a, b) => b.name.localeCompare(a.name))
						: [...items].sort((a, b) => a.name.localeCompare(b.name));
				},
			},
			{
				name: 'Last login',
				sort: (items: Array<UserItem>, desc: boolean) => {
					return desc
						? [...items].sort((a, b) => +new Date(b.lastLogin) - +new Date(a.lastLogin))
						: [...items].sort((a, b) => +new Date(a.lastLogin) - +new Date(b.lastLogin));
				},
			},
			{
				name: 'status',
				sort: (items: Array<UserItem>, desc: boolean) => {
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

	disconnectedCallback(): void {
		super.disconnectedCallback();

		this._usersSubscription?.unsubscribe();
	}

	private _selectAllHandler(event: Event) {
		const checkboxElement = event.target as HTMLInputElement;
		this._selection = checkboxElement.checked ? this._users.map((item: UserItem) => item.key) : [];
		this._selectionMode = this._selection.length > 0;
	}

	private _selectHandler(event: Event, item: UserItem) {
		const checkboxElement = event.target as HTMLInputElement;
		this._selection = checkboxElement.checked
			? [...this._selection, item.key]
			: this._selection.filter((selectionKey) => selectionKey !== item.key);
		this._selectionMode = this._selection.length > 0;
	}
	private _selectRowHandler(item: UserItem) {
		this._selection = [...this._selection, item.key];
		this._selectionMode = this._selection.length > 0;
	}
	private _unselectRowHandler(item: UserItem) {
		this._selection = this._selection.filter((selectionKey) => selectionKey !== item.key);
		this._selectionMode = this._selection.length > 0;
	}

	private _sortingHandler(column: TableColumn) {
		this._sortingDesc = this._sortingColumn === column.name ? !this._sortingDesc : false;
		this._sortingColumn = column.name;
		this._users = column.sort(this._users, this._sortingDesc);
	}

	private _isSelected(key: string) {
		return this._selection.includes(key);
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

	protected renderRowTemplate = (user: UserItem) => {
		if (!this._usersContext) return;

		const statusLook = this._usersContext.getTagLookAndColor(user.status ? user.status : '');

		return html`<uui-table-row
			selectable
			?select-only=${this._selectionMode}
			?selected=${this._isSelected(user.key)}
			@selected=${() => this._selectRowHandler(user)}
			@unselected=${() => this._unselectRowHandler(user)}>
			<uui-table-cell>
				<div style="display: flex; align-items: center;">
					<uui-avatar name="${user.name}"></uui-avatar>
				</div>
			</uui-table-cell>
			<uui-table-cell>
				<div style="display: flex; align-items: center;">
					<a style="font-weight: bold;" href="http://">${user.name}</a>
				</div>
			</uui-table-cell>
			<uui-table-cell> ${user.userGroup} </uui-table-cell>
			<uui-table-cell>${user.lastLogin}</uui-table-cell>
			<uui-table-cell>
				${user.status
					? html`<uui-tag size="s" look="${statusLook.look}" color="${statusLook.color}"> ${user.status} </uui-tag>`
					: nothing}
			</uui-table-cell>
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
							?checked="${this._selection.length === this._users.length}"></uui-checkbox>
					</uui-table-head-cell>
					${this._columns.map((column) => this.renderHeaderCellTemplate(column))}
				</uui-table-head>
				${repeat(this._users, (item) => item.key, this.renderRowTemplate)}
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
