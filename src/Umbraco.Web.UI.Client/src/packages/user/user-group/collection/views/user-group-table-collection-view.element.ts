import { UMB_USER_GROUP_COLLECTION_CONTEXT } from '../user-group-collection.context-token.js';
import type { UmbUserGroupDetailModel } from '../../types.js';
import type { UmbUserGroupCollectionContext } from '../user-group-collection.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableElement,
	UmbTableItem,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbUniqueItemModel } from '@umbraco-cms/backoffice/models';

import '../components/user-group-table-name-column-layout.element.js';
import '../components/user-group-table-sections-column-layout.element.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';

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
		{
			name: '',
			alias: 'entityActions',
			align: 'right',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	@state()
	private _selection: Array<string | null> = [];

	#collectionContext?: UmbUserGroupCollectionContext;

	// TODO: hardcoded dependencies on document and media modules. We should figure out how these dependencies can be added through extensions.
	#documentItemRepository?: UmbItemRepository<UmbUniqueItemModel>;
	#mediaItemRepository?: UmbItemRepository<UmbUniqueItemModel>;

	#documentStartNodeMap = new Map<string, string>();
	#mediaStartNodeMap = new Map<string, string>();

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.observe(
				this.#collectionContext?.selection.selection,
				(selection) => (this._selection = selection ?? []),
				'umbCollectionSelectionObserver',
			);
			this.observe(
				this.#collectionContext?.items,
				async (items) => {
					await this.#initRepositories();
					this._createTableItems(items ?? []);
				},
				'umbCollectionItemsObserver',
			);
		});
	}

	async #initRepositories() {
		if (this.#documentItemRepository && this.#mediaItemRepository) return;

		// TODO: get back to this when documents have been decoupled from users.
		// The repository alias is hardcoded on purpose to avoid a document import in the user module.
		this.#documentItemRepository = await createExtensionApiByAlias<UmbItemRepository<UmbUniqueItemModel>>(
			this,
			'Umb.Repository.DocumentItem',
		);

		// TODO: get back to this when media have been decoupled from users.
		// The repository alias is hardcoded on purpose to avoid a media import in the user module.
		this.#mediaItemRepository = await createExtensionApiByAlias<UmbItemRepository<UmbUniqueItemModel>>(
			this,
			'Umb.Repository.MediaItem',
		);
	}

	private async _createTableItems(userGroups: Array<UmbUserGroupDetailModel>) {
		if (!this.#documentItemRepository || !this.#mediaItemRepository) {
			throw new Error('Document and media item repositories are not initialized.');
		}

		await Promise.all([
			this.#requestAndCacheStartNodes(
				userGroups,
				'documentStartNode',
				this.#documentItemRepository,
				this.#documentStartNodeMap,
			),
			this.#requestAndCacheStartNodes(userGroups, 'mediaStartNode', this.#mediaItemRepository, this.#mediaStartNodeMap),
		]);

		this._tableItems = userGroups.map((userGroup) => {
			return {
				id: userGroup.unique,
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
						value: userGroup.documentStartNode
							? this.#documentStartNodeMap.get(userGroup.documentStartNode.unique)
							: this.localize.term('content_contentRoot'),
					},
					{
						columnAlias: 'userGroupMediaStartNode',
						value: userGroup.mediaStartNode?.unique
							? this.#mediaStartNodeMap.get(userGroup.mediaStartNode.unique)
							: this.localize.term('media_mediaRoot'),
					},
					{
						columnAlias: 'entityActions',
						value: html`<umb-entity-actions-table-column-view
							.value=${{
								entityType: userGroup.entityType,
								unique: userGroup.unique,
								name: userGroup.name,
							}}></umb-entity-actions-table-column-view>`,
					},
				],
			};
		});
	}

	async #requestAndCacheStartNodes(
		userGroups: Array<UmbUserGroupDetailModel>,
		startNodeField: 'documentStartNode' | 'mediaStartNode',
		itemRepository: UmbItemRepository<UmbUniqueItemModel>,
		map: Map<string, string>,
	) {
		const allStartNodes = userGroups.map((userGroup) => userGroup[startNodeField]?.unique).filter(Boolean) as string[];
		const uniqueStartNodes = [...new Set(allStartNodes)];
		const uncachedStartNodes = uniqueStartNodes.filter((unique) => !map.has(unique));

		// If there are no uncached start nodes, we don't need to make a request
		if (uncachedStartNodes.length === 0) return;

		const { data: items } = await itemRepository.requestItems(uncachedStartNodes);

		if (items) {
			items.forEach((item) => {
				// cache the start node
				map.set(item.unique, item.name);
			});
		}
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

	override render() {
		return html`
			<umb-table
				.config=${this._tableConfig}
				.columns=${this._tableColumns}
				.items=${this._tableItems}
				.selection=${this._selection}
				@selected="${this.#onSelected}"
				@deselected="${this.#onDeselected}"></umb-table>
		`;
	}

	static override styles = [UmbTextStyles];
}

export default UmbUserGroupCollectionTableViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-collection-table-view': UmbUserGroupCollectionTableViewElement;
	}
}
