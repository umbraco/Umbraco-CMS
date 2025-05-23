import { UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentTreeServerDataSource } from './document-tree.server.data-source.js';
import type { UmbDocumentTreeItemModel, UmbDocumentTreeRootModel } from './types.js';
import { UMB_DOCUMENT_TREE_STORE_CONTEXT } from './document-tree.store.context-token.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentTreeRepository
	extends UmbTreeRepositoryBase<UmbDocumentTreeItemModel, UmbDocumentTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTreeServerDataSource, UMB_DOCUMENT_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 1 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbDocumentTreeRootModel = {
			unique: null,
			entityType: UMB_DOCUMENT_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_content',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export default UmbDocumentTreeRepository;
