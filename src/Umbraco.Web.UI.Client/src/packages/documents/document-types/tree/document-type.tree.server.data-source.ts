import { UMB_DOCUMENT_TYPE_ENTITY_TYPE, UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentTypeTreeItemModel } from './types.js';
import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from './folder/index.js';
import { UmbManagementApiDocumentTypeTreeDataRequestManager } from './server-data-source/document-type-tree.server.request-manager.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { DocumentTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Document Type tree that fetches data from the server
 * @class UmbDocumentTypeTreeServerDataSource
 * @augments {UmbTreeServerDataSourceBase}
 */
export class UmbDocumentTypeTreeServerDataSource extends UmbTreeServerDataSourceBase<
	DocumentTypeTreeItemResponseModel,
	UmbDocumentTypeTreeItemModel
> {
	#treeRequestManager = new UmbManagementApiDocumentTypeTreeDataRequestManager(this);

	/**
	 * Creates an instance of UmbDocumentTypeTreeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args: UmbTreeRootItemsRequestArgs) => this.#treeRequestManager.getRootItems(args),
			getChildrenOf: (args: UmbTreeChildrenOfRequestArgs) => this.#treeRequestManager.getChildrenOf(args),
			getAncestorsOf: (args: UmbTreeAncestorsOfRequestArgs) => this.#treeRequestManager.getAncestorsOf(args),
			mapper: (item: DocumentTypeTreeItemResponseModel) => {
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
			},
		});
	}
}
