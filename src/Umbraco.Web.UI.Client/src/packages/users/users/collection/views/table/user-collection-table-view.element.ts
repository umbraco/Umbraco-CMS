import { UmbUserCollectionContext } from '../../user-collection.context.js';
import type { UmbUserDetail } from '../../../types.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	UmbTableElement,
	UmbTableColumn,
	UmbTableDeselectedEvent,
	UmbTableItem,
	UmbTableSelectedEvent,
	UmbTableConfig,
	UmbTableOrderedEvent,
} from '@umbraco-cms/backoffice/components';
import { UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbUserGroupRepository } from '@umbraco-cms/backoffice/user-group';

import './column-layouts/name/user-table-name-column-layout.element.js';
import './column-layouts/status/user-table-status-column-layout.element.js';
import { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-user-collection-table-view')
export class UmbUserCollectionTableViewElement extends UmbLitElement {
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
	private _userGroupItems: Array<UserGroupItemResponseModel> = [];

	#UmbUserGroupRepository = new UmbUserGroupRepository(this);

	@state()
	private _users: Array<UmbUserDetail> = [];

	@state()
	private _selection: Array<string> = [];

	#collectionContext?: UmbUserCollectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this.#collectionContext = instance as UmbUserCollectionContext;
			this.observe(this.#collectionContext.selection, (selection) => (this._selection = selection));
			this.observe(this.#collectionContext.items, (items) => {
				this._users = items;
				this.#observeUserGroups();
			});
		});
	}

	async #observeUserGroups() {
		if (this._users.length === 0) return;
		const userGroupsIds = [...new Set(this._users.flatMap((user) => user.userGroupIds ?? []))];
		const { asObservable } = await this.#UmbUserGroupRepository.requestItems(userGroupsIds);
		this.observe(asObservable(), (userGroups) => {
			this._userGroupItems = userGroups;
			this.#createTableItems();
		});
	}

	#getUserGroupNames(ids: Array<string>) {
		return ids
			.map((id: string) => {
				return this._userGroupItems.find((x) => x.id === id)?.name;
			})
			.join(', ');
	}

	#createTableItems() {
		this._tableItems = this._users.map((user) => {
			return {
				id: user.id ?? '',
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
						value: this.#getUserGroupNames(user.userGroupIds ?? []),
					},
					{
						columnAlias: 'userLastLogin',
						value: user.lastLoginDate,
					},
					{
						columnAlias: 'userStatus',
						value: {
							status: user.state,
						},
					},
				],
			};
		});
	}

	#onSelected(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#collectionContext?.setSelection(selection);
	}

	#onDeselected(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#collectionContext?.setSelection(selection);
	}

	#onOrdering(event: UmbTableOrderedEvent) {
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
				@selected="${this.#onSelected}"
				@deselected="${this.#onDeselected}"
				@ordered="${this.#onOrdering}"></umb-table>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			umb-table {
				padding: 0;
				margin: 0 var(--uui-size-layout-1) var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbUserCollectionTableViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-collection-table-view': UmbUserCollectionTableViewElement;
	}
}
