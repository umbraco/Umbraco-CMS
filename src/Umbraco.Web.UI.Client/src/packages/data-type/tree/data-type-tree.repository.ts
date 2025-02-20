import { UMB_DATA_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDataTypeTreeServerDataSource } from './data-type-tree.server.data-source.js';
import { UMB_DATA_TYPE_TREE_STORE_CONTEXT } from './data-type-tree.store.context-token.js';
import type { UmbDataTypeTreeItemModel, UmbDataTypeTreeRootModel } from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDataTypeTreeRepository
	extends UmbTreeRepositoryBase<UmbDataTypeTreeItemModel, UmbDataTypeTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeTreeServerDataSource, UMB_DATA_TYPE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 1 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbDataTypeTreeRootModel = {
			unique: null,
			entityType: UMB_DATA_TYPE_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_dataTypes',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export { UmbDataTypeTreeRepository as api };
