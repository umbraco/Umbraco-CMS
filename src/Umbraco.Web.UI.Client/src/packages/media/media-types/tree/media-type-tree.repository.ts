import { UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMediaTypeTreeServerDataSource } from './media-type-tree.server.data-source.js';
import { UMB_MEDIA_TYPE_TREE_STORE_CONTEXT } from './media-type-tree.store.context-token.js';
import type { UmbMediaTypeTreeItemModel, UmbMediaTypeTreeRootModel } from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaTypeTreeRepository
	extends UmbTreeRepositoryBase<UmbMediaTypeTreeItemModel, UmbMediaTypeTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTypeTreeServerDataSource, UMB_MEDIA_TYPE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 1 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbMediaTypeTreeRootModel = {
			unique: null,
			entityType: UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_mediaTypes',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export default UmbMediaTypeTreeRepository;
