import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSelectedEvent,
} from '../../../shared/components/table';
import { UmbUserGroupStore, UMB_USER_GROUP_STORE_CONTEXT_TOKEN } from '../repository/user-group.store';
import type { UserGroupDetails } from '../types';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './user-group-table-name-column-layout.element';
//import '../../user-section/views/user-groups/user-group-table-sections-column-layout.element';

@customElement('umb-workspace-view-user-groups')
export class UmbWorkspaceViewUserGroupsElement extends UmbLitElement {
	@state()
	private _userGroups: Array<UserGroupDetails> = [];

	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: true,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Name',
			alias: 'userGroupName',
			elementName: 'umb-user-group-table-name-column-layout',
		},
		{
			name: 'Sections',
			alias: 'userGroupSections',
			elementName: 'umb-user-group-table-sections-column-layout',
		},
		{
			name: 'Content start node',
			alias: 'userGroupContentStartNode',
		},
		{
			name: 'Media start node',
			alias: 'userGroupMediaStartNode',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	@state()
	private _selection: Array<string> = [];

	private _userGroupStore?: UmbUserGroupStore;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext(UMB_USER_GROUP_STORE_CONTEXT_TOKEN, (userGroupStore) => {
			this._userGroupStore = userGroupStore;
			this._observeUserGroups();
		});
	}

	private _observeUserGroups() {
		if (!this._userGroupStore) return;

		this.observe(this._userGroupStore.getAll(), (userGroups) => {
			this._userGroups = userGroups;
			this._createTableItems(this._userGroups);
		});
	}

	private _createTableItems(userGroups: Array<UserGroupDetails>) {
		this._tableItems = userGroups.map((userGroup) => {
			return {
				id: userGroup.id || '',
				icon: userGroup.icon,
				data: [
					{
						columnAlias: 'userGroupName',
						value: {
							name: userGroup.name,
						},
					},
					{
						columnAlias: 'userGroupSections',
						value: userGroup.sections,
					},
					{
						columnAlias: 'userGroupContentStartNode',
						value: userGroup.contentStartNode || 'Content root',
					},
					{
						columnAlias: 'userGroupMediaStartNode',
						value: userGroup.mediaStartNode || 'Media root',
					},
				],
			};
		});
	}

	private _handleSelected(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		console.log('HANDLE SELECT');
	}

	private _handleDeselected(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		console.log('HANDLE DESELECT');
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

	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
				display: flex;
				flex-direction: column;
				margin: var(--uui-size-layout-1);
			}

			umb-table {
				padding: 0;
			}
		`,
	];
}

export default UmbWorkspaceViewUserGroupsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-user-groups': UmbWorkspaceViewUserGroupsElement;
	}
}
