import { UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMediaTreeServerDataSource } from './media-tree.server.data-source.js';
import type { UmbMediaTreeItemModel, UmbMediaTreeRootModel } from './types.js';
import { UMB_MEDIA_TREE_STORE_CONTEXT } from './media-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaTreeRepository
	extends UmbTreeRepositoryBase<UmbMediaTreeItemModel, UmbMediaTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTreeServerDataSource, UMB_MEDIA_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data: UmbMediaTreeRootModel = {
			unique: null,
			entityType: UMB_MEDIA_ROOT_ENTITY_TYPE,
			name: 'Media',
			hasChildren: true,
			isFolder: true,
		};

		return { data };
	}
}
