import { UMB_STATIC_FILE_ROOT_ENTITY_TYPE } from './constants.js';
import { UmbStaticFileTreeServerDataSource } from './static-file-tree.server.data-source.js';
import type { UmbStaticFileTreeItemModel, UmbStaticFileTreeRootModel } from './types.js';
import { UMB_STATIC_FILE_TREE_STORE_CONTEXT } from './static-file-tree.store.context-token.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbStaticFileTreeRepository
	extends UmbTreeRepositoryBase<UmbStaticFileTreeItemModel, UmbStaticFileTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbStaticFileTreeServerDataSource, UMB_STATIC_FILE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 1 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbStaticFileTreeRootModel = {
			unique: null,
			entityType: UMB_STATIC_FILE_ROOT_ENTITY_TYPE,
			name: 'Static Files',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export default UmbStaticFileTreeRepository;
