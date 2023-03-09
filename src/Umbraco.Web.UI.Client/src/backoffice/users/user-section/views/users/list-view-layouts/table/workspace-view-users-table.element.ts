import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import type { UmbSectionViewUsersElement } from '../../section-view-users.element';
import {
	UmbTableElement,
	UmbTableColumn,
	UmbTableDeselectedEvent,
	UmbTableItem,
	UmbTableSelectedEvent,
	UmbTableConfig,
	UmbTableOrderedEvent,
} from '../../../../../../shared/components/table/table.element';
import type { UserDetails, UserGroupEntity } from '@umbraco-cms/models';

import './column-layouts/name/user-table-name-column-layout.element';
import './column-layouts/status/user-table-status-column-layout.element';
import {
	UmbUserGroupStore,
	UMB_USER_GROUP_STORE_CONTEXT_TOKEN,
} from 'src/backoffice/users/user-groups/repository/user-group.store';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-workspace-view-users-table')
export class UmbWorkspaceViewUsersTableElement extends UmbLitElement {
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

	@state()
	private _userGroups: Array<UserGroupEntity> = [];

	private _userGroupStore?: UmbUserGroupStore;
	private _usersContext?: UmbSectionViewUsersElement;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_STORE_CONTEXT_TOKEN, (instance) => {
			this._userGroupStore = instance;
			this._observeUserGroups();
		});

		this.consumeContext<UmbSectionViewUsersElement>('umbUsersContext', (_instance) => {
			this._usersContext = _instance;
			this._observeUsers();
			this._observeSelection();
		});
	}

	private _observeUsers() {
		if (!this._usersContext) return;
		this.observe(this._usersContext.users, (users) => {
			this._users = users;
			this._createTableItems(this._users);
		});
	}

	private _observeSelection() {
		if (!this._usersContext) return;
		this.observe(this._usersContext.selection, (selection) => {
			if (this._selection === selection) return;
			this._selection = selection;
		});
	}

	private _observeUserGroups() {
		if (!this._userGroupStore) return;
		this.observe(this._userGroupStore.getAll(), (userGroups) => {
			this._userGroups = userGroups;
			this._createTableItems(this._users);
		});
	}

	private _getUserGroupNames(keys: Array<string>) {
		return keys
			.map((key: string) => {
				return this._userGroups.find((x) => x.key === key)?.name;
			})
			.join(', ');
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
						value: this._getUserGroupNames(user.userGroups),
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

export default UmbWorkspaceViewUsersTableElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-users-table': UmbWorkspaceViewUsersTableElement;
	}
}
