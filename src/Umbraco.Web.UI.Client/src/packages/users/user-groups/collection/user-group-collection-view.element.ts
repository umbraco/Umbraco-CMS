import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbUserGroupCollectionContext } from './user-group-collection.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';

import './user-group-table-name-column-layout.element.js';
import './user-group-table-sections-column-layout.element.js';
import {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';

@customElement('umb-user-group-collection-view')
export class UmbUserGroupCollectionViewElement extends UmbLitElement {
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

	@state()
	private _userGroups: Array<UserGroupResponseModel> = [];

	#collectionContext?: UmbUserGroupCollectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this.#collectionContext = instance;
			this.observe(this.#collectionContext.selection, (selection) => (this._selection = selection));
			this.observe(this.#collectionContext.items, (items) => {
				this._userGroups = items;
				this._createTableItems(items);
			});
		});
	}

	private _createTableItems(userGroups: Array<UserGroupResponseModel>) {
		this._tableItems = userGroups.map((userGroup) => {
			return {
				id: userGroup.id || '',
				icon: userGroup.icon || '',
				data: [
					{
						columnAlias: 'userGroupName',
						value: {
							name: userGroup.name || '',
						},
					},
					{
						columnAlias: 'userGroupSections',
						value: userGroup.sections || [],
					},
					{
						columnAlias: 'userGroupContentStartNode',
						value: userGroup.documentStartNodeId || 'Content root',
					},
					{
						columnAlias: 'userGroupMediaStartNode',
						value: userGroup.mediaStartNodeId || 'Media root',
					},
				],
			};
		});
	}

	private _handleSelected(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#collectionContext?.setSelection(selection);
	}

	private _handleDeselected(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#collectionContext?.setSelection(selection);
	}

	render() {
		return html`
			<umb-table
				.config=${this._tableConfig}
				.columns=${this._tableColumns}
				.items=${this._tableItems}
				.selection=${this._selection}
				@selected="${this._handleSelected}"
				@deselected="${this._handleDeselected}"></umb-table>
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

export default UmbUserGroupCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-collection-view': UmbUserGroupCollectionViewElement;
	}
}
