import { UmbUserGroupCollectionContext } from '../user-group-collection.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';

import '../components/user-group-table-name-column-layout.element.js';
import '../components/user-group-table-sections-column-layout.element.js';
import {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';

@customElement('umb-user-group-collection-table-view')
export class UmbUserGroupCollectionTableViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: true,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: this.localize.term('general_name'),
			alias: 'userGroupName',
			elementName: 'umb-user-group-table-name-column-layout',
		},
		{
			name: this.localize.term('main_sections'),
			alias: 'userGroupSections',
			elementName: 'umb-user-group-table-sections-column-layout',
		},
		{
			name: this.localize.term('user_startnode'),
			alias: 'userGroupContentStartNode',
		},
		{
			name: this.localize.term('user_mediastartnode'),
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
						value: userGroup.documentStartNodeId || this.localize.term('content_contentRoot'),
					},
					{
						columnAlias: 'userGroupMediaStartNode',
						value: userGroup.mediaStartNodeId || this.localize.term('media_mediaRoot'),
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
		UmbTextStyles,
		css`
			umb-table {
				padding: 0;
			}
		`,
	];
}

export default UmbUserGroupCollectionTableViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-collection-table-view': UmbUserGroupCollectionTableViewElement;
	}
}
