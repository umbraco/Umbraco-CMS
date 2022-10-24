import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import type { UserGroupDetails } from '../../../../../core/models';
import { UmbUserGroupStore } from '../../../../../core/stores/user/user-group.store';
import UmbTableElement, {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSelectedEvent,
} from '../../../../components/table/table.element';

import './user-group-table-name-column-layout.element';

@customElement('umb-editor-view-user-groups')
export class UmbEditorViewUserGroupsElement extends UmbContextConsumerMixin(LitElement) {
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
	private _userGroupsSubscription?: Subscription;
	private _selectionSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUserGroupStore', (userStore: UmbUserGroupStore) => {
			this._userGroupStore = userStore;
			this._observeUserGroups();
		});
	}

	private _observeUserGroups() {
		this._userGroupsSubscription?.unsubscribe();
		this._userGroupsSubscription = this._userGroupStore?.getAll().subscribe((userGroups) => {
			this._userGroups = userGroups;
			this._createTableItems(this._userGroups);
		});
	}

	private _createTableItems(userGroups: Array<UserGroupDetails>) {
		this._tableItems = userGroups.map((userGroup) => {
			return {
				key: userGroup.key,
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
						value: userGroup.contentStartNode,
					},
					{
						columnAlias: 'userGroupMediaStartNode',
						value: userGroup.mediaStartNode,
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

	disconnectedCallback(): void {
		super.disconnectedCallback();

		this._userGroupsSubscription?.unsubscribe();
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

export default UmbEditorViewUserGroupsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-user-groups': UmbEditorViewUserGroupsElement;
	}
}
