import { UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../constants.js';
import { UmbDocumentRecycleBinTreeServerDataSource } from './document-recycle-bin-tree.server.data-source.js';
import type { UmbDocumentRecycleBinTreeItemModel, UmbDocumentRecycleBinTreeRootModel } from './types.js';
import { UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_CONTEXT } from './document-recycle-bin-tree.store.context-token.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentRecycleBinTreeRepository
	extends UmbTreeRepositoryBase<UmbDocumentRecycleBinTreeItemModel, UmbDocumentRecycleBinTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentRecycleBinTreeServerDataSource, UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 1 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data = {
			unique: null,
			entityType: UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_contentRecycleBin',
			icon: 'icon-trash',
			hasChildren,
			isContainer: false,
			isFolder: true,
		};

		return { data };
	}
}

export { UmbDocumentRecycleBinTreeRepository as api };
