import type { UmbUserCollectionContext } from '../../user-collection.context.js';
import type { UmbUserDetailModel } from '../../../types.js';
import { UMB_USER_COLLECTION_CONTEXT } from '../../user-collection.context-token.js';
import { UmbUserKind } from '../../../utils/index.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbTableElement,
	UmbTableColumn,
	UmbTableDeselectedEvent,
	UmbTableItem,
	UmbTableSelectedEvent,
	UmbTableConfig,
	UmbTableOrderedEvent,
} from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbUserGroupItemModel } from '@umbraco-cms/backoffice/user-group';
import { UmbUserGroupItemRepository } from '@umbraco-cms/backoffice/user-group';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

import './column-layouts/name/user-table-name-column-layout.element.js';
import './column-layouts/status/user-table-status-column-layout.element.js';

@customElement('umb-user-table-collection-view')
export class UmbUserTableCollectionViewElement extends UmbLitElement {
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
		{
			name: '',
			alias: 'entityActions',
			align: 'right',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	@state()
	private _userGroupItems: Array<UmbUserGroupItemModel> = [];

	#userGroupItemRepository = new UmbUserGroupItemRepository(this);

	@state()
	private _users: Array<UmbUserDetailModel> = [];

	@state()
	private _selection: Array<string | null> = [];

	#collectionContext?: UmbUserCollectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_USER_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.observe(
				this.#collectionContext?.selection.selection,
				(selection) => (this._selection = selection ?? []),
				'umbCollectionSelectionObserver',
			);
			this.observe(
				this.#collectionContext?.items,
				(items) => {
					this._users = items ?? [];
					this.#observeUserGroups();
				},
				'umbCollectionItemsObserver',
			);
		});
	}

	async #observeUserGroups() {
		if (this._users.length === 0) return;
		const userGroupsUniques = [
			...new Set(this._users.flatMap((user) => user.userGroupUniques.map((reference) => reference.unique))),
		];
		const { asObservable } = await this.#userGroupItemRepository.requestItems(userGroupsUniques);
		this.observe(
			asObservable(),
			(userGroups) => {
				this._userGroupItems = userGroups;
				this.#createTableItems();
			},
			'umbUserGroupItemsObserver',
		);
	}

	#getUserGroupNames(references: Array<UmbReferenceByUnique>) {
		return references
			.map((reference) => {
				return this._userGroupItems.find((x) => x.unique === reference.unique)?.name;
			})
			.join(', ');
	}

	#createTableItems() {
		this._tableItems = this._users.map((user) => {
			return {
				id: user.unique,
				icon: user.kind === UmbUserKind.API ? 'icon-unplug' : 'icon-user',
				data: [
					{
						columnAlias: 'userName',
						value: {
							unique: user.unique,
							name: user.name,
							avatarUrls: user.avatarUrls,
							kind: user.kind,
						},
					},
					{
						columnAlias: 'userGroup',
						value: this.#getUserGroupNames(user.userGroupUniques ?? []),
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
					{
						columnAlias: 'entityActions',
						value: html`<umb-entity-actions-table-column-view
							.value=${{
								entityType: user.entityType,
								unique: user.unique,
							}}></umb-entity-actions-table-column-view>`,
					},
				],
			};
		});
	}

	#onSelected(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#collectionContext?.selection.setSelection(selection);
	}

	#onDeselected(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#collectionContext?.selection.setSelection(selection);
	}

	#onOrdering(event: UmbTableOrderedEvent) {
		const table = event.target as UmbTableElement;
		const orderingColumn = table.orderingColumn;
		const orderingDesc = table.orderingDesc;
		console.log(`fetch users, order column: ${orderingColumn}, desc: ${orderingDesc}`);
	}

	override render() {
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

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

export default UmbUserTableCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-table-collection-view': UmbUserTableCollectionViewElement;
	}
}
