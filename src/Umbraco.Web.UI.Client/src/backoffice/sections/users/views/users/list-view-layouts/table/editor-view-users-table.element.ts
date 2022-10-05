import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../../../core/context';
import { repeat } from 'lit/directives/repeat.js';
import { Subscription } from 'rxjs';
import UmbSectionViewUsersElement from '../../section-view-users.element';
import { UmbUserStore } from '../../../../../../../core/stores/user/user.store';
import type { UserEntity } from '../../../../../../../core/models';

interface TableColumn {
	name: string;
	sort: (items: Array<UserEntity>, desc: boolean) => Array<UserEntity>;
}

@customElement('umb-editor-view-users-table')
export class UmbEditorViewUsersTableElement extends UmbContextConsumerMixin(LitElement) {
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
			.link-button {
				font-weight: bold;
				color: var(--uui-color-interactive);
			}
			.link-button:hover {
				text-decoration: underline;
				color: var(--uui-color-interactive-emphasis);
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
	private _users: Array<UserEntity> = [];

	private _userStore?: UmbUserStore;
	private _usersContext?: UmbSectionViewUsersElement;
	private _usersSubscription?: Subscription;
	private _selectionSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUserStore', (userStore: UmbUserStore) => {
			this._userStore = userStore;
			this._observeUsers();
		});

		this.consumeContext('umbUsersContext', (usersContext: UmbSectionViewUsersElement) => {
			this._usersContext = usersContext;
			this._observeSelection();
		});

		this._columns = [
			{
				name: 'Name',
				sort: (items: Array<UserEntity>, desc: boolean) => {
					return desc
						? [...items].sort((a, b) => b.name.localeCompare(a.name))
						: [...items].sort((a, b) => a.name.localeCompare(b.name));
				},
			},
			{
				name: 'User group',
				sort: (items: Array<UserEntity>, desc: boolean) => {
					return desc
						? [...items].sort((a, b) => b.name.localeCompare(a.name))
						: [...items].sort((a, b) => a.name.localeCompare(b.name));
				},
			},
			{
				name: 'Last login',
				sort: (items: Array<UserEntity>, desc: boolean) => {
					return desc
						? [...items].sort((a, b) => +new Date(b.lastLoginDate || 0) - +new Date(a.lastLoginDate || 0))
						: [...items].sort((a, b) => +new Date(a.lastLoginDate || 0) - +new Date(b.lastLoginDate || 0));
				},
			},
			{
				name: 'status',
				sort: (items: Array<UserEntity>, desc: boolean) => {
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
		this._selectionSubscription?.unsubscribe();
	}

	private _observeUsers() {
		this._usersSubscription?.unsubscribe();
		this._usersSubscription = this._userStore?.getAll().subscribe((users) => {
			this._users = users;
		});
	}

	private _observeSelection() {
		this._selectionSubscription?.unsubscribe();
		this._selectionSubscription = this._usersContext?.selection.subscribe((selection: Array<string>) => {
			this._selection = selection;
		});
	}

	private _selectAllHandler(event: Event) {
		console.log('SELECT ALL NOT IMPLEMENTED');
	}

	//TODO How should we handle url stuff?
	private _handleOpenUser(event: Event, key: string) {
		event.stopImmediatePropagation();
		history.pushState(null, '', '/section/users/view/users/details' + '/' + key); //TODO: make a tag with href
	}

	private _selectRowHandler(user: UserEntity) {
		this._usersContext?.select(user.key);
	}

	private _deselectRowHandler(user: UserEntity) {
		this._usersContext?.deselect(user.key);
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

	protected renderRowTemplate = (user: UserEntity) => {
		if (!this._usersContext) return;

		const statusLook = this._usersContext.getTagLookAndColor(user.status ? user.status : '');

		return html`<uui-table-row
			selectable
			?select-only=${this._selectionMode}
			?selected=${this._isSelected(user.key)}
			@selected=${() => this._selectRowHandler(user)}
			@unselected=${() => this._deselectRowHandler(user)}>
			<uui-table-cell>
				<div style="display: flex; align-items: center;">
					<uui-avatar name="${user.name}"></uui-avatar>
				</div>
			</uui-table-cell>
			<uui-table-cell>
				<div style="display: flex; align-items: center;">
					<div
						class="link-button"
						@keydown=${(e: KeyboardEvent) => {
							if (e.key === 'Enter') {
								this._handleOpenUser(e, user.key);
							}
						}}
						@click=${(e: Event) => this._handleOpenUser(e, user.key)}
						href=${location.pathname + '/' + user.key}>
						${user.name}
					</div>
				</div>
			</uui-table-cell>
			<uui-table-cell> ${user.userGroup} </uui-table-cell>
			<uui-table-cell>${user.lastLoginDate}</uui-table-cell>
			<uui-table-cell>
				${user.status && user.status !== 'Enabled'
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

export default UmbEditorViewUsersTableElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-table': UmbEditorViewUsersTableElement;
	}
}
