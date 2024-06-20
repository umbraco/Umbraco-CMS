import { UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentTypeTreeServerDataSource } from './document-type.tree.server.data-source.js';
import { UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT } from './document-type.tree.store.context-token.js';
import type { UmbDocumentTypeTreeItemModel, UmbDocumentTypeTreeRootModel } from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentTypeTreeRepository
	extends UmbTreeRepositoryBase<UmbDocumentTypeTreeItemModel, UmbDocumentTypeTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTypeTreeServerDataSource, UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 1 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbDocumentTypeTreeRootModel = {
			unique: null,
			entityType: UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_documentTypes',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export { UmbDocumentTypeTreeRepository as api };
