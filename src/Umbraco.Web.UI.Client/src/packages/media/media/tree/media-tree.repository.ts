import { UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMediaTreeServerDataSource } from './media-tree.server.data-source.js';
import type {
	UmbMediaTreeChildrenOfRequestArgs,
	UmbMediaTreeItemModel,
	UmbMediaTreeRootItemsRequestArgs,
	UmbMediaTreeRootModel,
} from './types.js';
import { UMB_MEDIA_TREE_STORE_CONTEXT } from './media-tree.store.context-token.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaTreeRepository
	extends UmbTreeRepositoryBase<
		UmbMediaTreeItemModel,
		UmbMediaTreeRootModel,
		UmbMediaTreeRootItemsRequestArgs,
		UmbMediaTreeChildrenOfRequestArgs
	>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTreeServerDataSource, UMB_MEDIA_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 1 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbMediaTreeRootModel = {
			unique: null,
			entityType: UMB_MEDIA_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_media',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export default UmbMediaTreeRepository;
