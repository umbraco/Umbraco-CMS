import { UMB_DOCUMENT_TYPE_ENTITY_TYPE, UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../folder/constants.js';
import type { UmbDocumentTypeTreeItemModel } from '../types.js';
import { UmbManagementApiDocumentTypeTreeDataRequestManager } from './document-type-tree.server.request-manager.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { DocumentTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeDataSource,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Document Type tree that fetches data from the server
 * @class UmbDocumentTypeTreeServerDataSource
 */
export class UmbDocumentTypeTreeServerDataSource
	extends UmbControllerBase
	implements UmbTreeDataSource<UmbDocumentTypeTreeItemModel>
{
	#treeRequestManager = new UmbManagementApiDocumentTypeTreeDataRequestManager(this);

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

	#mapItem(item: DocumentTypeTreeItemResponseModel): UmbDocumentTypeTreeItemModel {
		return {
			unique: item.id,
			parent: {
				unique: item.parent ? item.parent.id : null,
				entityType: item.parent ? UMB_DOCUMENT_TYPE_ENTITY_TYPE : UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			},
			name: item.name,
			entityType: item.isFolder ? UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE : UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			hasChildren: item.hasChildren,
			isFolder: item.isFolder,
			icon: item.icon,
			isElement: item.isElement,
		};
	}
}
