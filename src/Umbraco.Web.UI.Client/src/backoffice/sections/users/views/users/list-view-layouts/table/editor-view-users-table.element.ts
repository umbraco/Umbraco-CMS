import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { UmbSectionViewUsersElement } from '../../section-view-users.element';
import { UmbUserStore } from '../../../../../../../core/stores/user/user.store';
import {
	UmbTableElement,
	UmbTableColumn,
	UmbTableDeselectedEvent,
	UmbTableItem,
	UmbTableSelectedEvent,
	UmbTableConfig,
	UmbTableOrderedEvent,
} from '../../../../../../components/table/table.element';
import type { UserDetails } from '@umbraco-cms/models';

import './column-layouts/name/user-table-name-column-layout.element';
import './column-layouts/status/user-table-status-column-layout.element';

@customElement('umb-editor-view-users-table')
export class UmbEditorViewUsersTableElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
				display: flex;
				flex-direction: column;
			}
		`,
	];

	@state()
	private _users: Array<UserDetails> = [];

	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: true,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Name',
			alias: 'userName',
			elementName: 'umb-user-table-name-column-layout',
		},
		{
			name: 'User group',
			alias: 'userGroup',
		},
		{
			name: 'Last login',
			alias: 'userLastLogin',
		},
		{
			name: 'Status',
			alias: 'userStatus',
			elementName: 'umb-user-table-status-column-layout',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	@state()
	private _selection: Array<string> = [];

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
	}

	private _observeUsers() {
		this._usersSubscription?.unsubscribe();
		this._usersSubscription = this._userStore?.getAll().subscribe((users) => {
			this._users = users;
			this._createTableItems(this._users);
		});
	}

	private _observeSelection() {
		this._selectionSubscription = this._usersContext?.selection.subscribe((selection: Array<string>) => {
			if (this._selection === selection) return;
			this._selection = selection;
		});
	}

	private _createTableItems(users: Array<UserDetails>) {
		this._tableItems = users.map((user) => {
			return {
				key: user.key,
				icon: 'umb:user',
				data: [
					{
						columnAlias: 'userName',
						value: {
							name: user.name,
						},
					},
					{
						columnAlias: 'userGroup',
						value: user.userGroup,
					},
					{
						columnAlias: 'userLastLogin',
						value: user.lastLoginDate,
					},
					{
						columnAlias: 'userStatus',
						value: {
							status: user.status,
						},
					},
				],
			};
		});
	}

	private _handleSelected(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this._usersContext?.setSelection(selection);
	}

	private _handleDeselected(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this._usersContext?.setSelection(selection);
	}

	private _handleOrdering(event: UmbTableOrderedEvent) {
		const table = event.target as UmbTableElement;
		const orderingColumn = table.orderingColumn;
		const orderingDesc = table.orderingDesc;
		console.log(`fetch users, order column: ${orderingColumn}, desc: ${orderingDesc}`);
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();

		this._usersSubscription?.unsubscribe();
		this._selectionSubscription?.unsubscribe();
	}

	render() {
		return html`
			<umb-table
				.config=${this._tableConfig}
				.columns=${this._tableColumns}
				.items=${this._tableItems}
				.selection=${this._selection}
				@selected="${this._handleSelected}"
				@deselected="${this._handleDeselected}"
				@ordered="${this._handleOrdering}"></umb-table>
		`;
	}
}

export default UmbEditorViewUsersTableElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-table': UmbEditorViewUsersTableElement;
	}
}
