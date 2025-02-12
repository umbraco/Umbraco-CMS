import { UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbPartialViewTreeServerDataSource } from './partial-view-tree.server.data-source.js';
import type { UmbPartialViewTreeItemModel, UmbPartialViewTreeRootModel } from './types.js';
import { UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT } from './partial-view-tree.store.context-token.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';

export class UmbPartialViewTreeRepository
	extends UmbTreeRepositoryBase<UmbPartialViewTreeItemModel, UmbPartialViewTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbPartialViewTreeServerDataSource, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 1 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbPartialViewTreeRootModel = {
			unique: null,
			entityType: UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_partialViews',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export default UmbPartialViewTreeRepository;
