import { UmbUserKind } from '../../../utils/index.js';
import type { UmbUserDetailModel } from '../../../types.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbTableElement,
	UmbTableColumn,
	UmbTableDeselectedEvent,
	UmbTableItem,
	UmbTableSelectedEvent,
	UmbTableOrderedEvent,
} from '@umbraco-cms/backoffice/components';

import type { UmbUserGroupItemModel } from '@umbraco-cms/backoffice/user-group';
import { UmbUserGroupItemRepository } from '@umbraco-cms/backoffice/user-group';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbCollectionViewElementBase } from '@umbraco-cms/backoffice/collection';

import './column-layouts/name/user-table-name-column-layout.element.js';
import './column-layouts/status/user-table-status-column-layout.element.js';

@customElement('umb-user-table-collection-view')
export class UmbUserTableCollectionViewElement extends UmbCollectionViewElementBase {
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
	protected override _items: Array<UmbUserDetailModel> = [];

	override updated(changedProperties: any) {
		if (changedProperties.has('_items')) {
			this.#createTableItems();
			this.#observeUserGroups();
		}
	}

	async #observeUserGroups() {
		if (this._items.length === 0) return;
		const userGroupsUniques = [
			...new Set(this._items.flatMap((user) => user.userGroupUniques.map((reference) => reference.unique))),
		];
		const { asObservable } = await this.#userGroupItemRepository.requestItems(userGroupsUniques);
		this.observe(
			asObservable?.(),
			(userGroups) => {
				this._userGroupItems = userGroups ?? [];
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
		this._tableItems = this._items.map((user) => {
			return {
				id: user.unique,
				icon: user.kind === UmbUserKind.API ? 'icon-unplug' : 'icon-user',
				selectable: this._isSelectableItem(user),
				data: [
					{
						columnAlias: 'userName',
						value: {
							unique: user.unique,
							name: user.name,
							avatarUrls: user.avatarUrls,
							kind: user.kind,
							href: user.unique ? this._itemHrefs.get(user.unique) : undefined,
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
								name: user.name,
							}}></umb-entity-actions-table-column-view>`,
					},
				],
			};
		});
	}

	#onSelected(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const itemId = event.getItemId();

		// We get the same event for both single and multiple selection.
		if (itemId) {
			this._selectItem(itemId);
		} else {
			const target = event.target as UmbTableElement;
			this._setSelection(target.selection);
		}
	}

	#onDeselected(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const itemId = event.getItemId();

		// We get the same event for both single and multiple deselection.
		if (itemId) {
			this._deselectItem(itemId);
		} else {
			const target = event.target as UmbTableElement;
			this._setSelection(target.selection);
		}
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
				.config=${{
					allowSelection: this._selectable,
					allowSelectAll: this._multiple,
					selectOnly: this._selectOnly,
				}}
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
