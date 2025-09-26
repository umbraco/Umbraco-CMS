import {
	UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
} from '../../entity.js';
import type { UmbDocumentBlueprintTreeItemModel } from '../types.js';
import { UmbManagementApiDocumentBlueprintTreeDataRequestManager } from './document-blueprint-tree.server.request-manager.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { DocumentBlueprintTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeDataSource,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Document Blueprint tree that fetches data from the server
 * @class UmbDocumentBlueprintTreeServerDataSource
 */
export class UmbDocumentBlueprintTreeServerDataSource
	extends UmbControllerBase
	implements UmbTreeDataSource<UmbDocumentBlueprintTreeItemModel>
{
	#treeRequestManager = new UmbManagementApiDocumentBlueprintTreeDataRequestManager(this);

	async getRootItems(args: UmbTreeRootItemsRequestArgs) {
		const { data, error } = await this.#treeRequestManager.getRootItems(args);

		const mappedData = data
			? {
					...data,
					items: data?.items.map((item) => this.#mapItem(item)),
				}
			: undefined;

		return { data: mappedData, error };
	}

	async getChildrenOf(args: UmbTreeChildrenOfRequestArgs) {
		const { data, error } = await this.#treeRequestManager.getChildrenOf(args);

		const mappedData = data
			? {
					...data,
					items: data?.items.map((item) => this.#mapItem(item)),
				}
			: undefined;

		return { data: mappedData, error };
	}

	async getAncestorsOf(args: UmbTreeAncestorsOfRequestArgs) {
		const { data, error } = await this.#treeRequestManager.getAncestorsOf(args);

		const mappedData = data?.map((item) => this.#mapItem(item));

		return { data: mappedData, error };
	}

	#mapItem(item: DocumentBlueprintTreeItemResponseModel): UmbDocumentBlueprintTreeItemModel {
		return {
			unique: item.id,
			parent: {
				unique: item.parent ? item.parent.id : null,
				entityType: item.parent ? UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE : UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
			},
			name: (item as any).variants?.[0].name ?? item.name,
			entityType: item.isFolder ? UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE : UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
			isFolder: item.isFolder,
			hasChildren: item.hasChildren,
			icon: item.isFolder ? 'icon-folder' : 'icon-blueprint',
		};
	}
}
