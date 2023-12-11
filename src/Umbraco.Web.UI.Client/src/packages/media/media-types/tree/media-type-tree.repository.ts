import { UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE } from '../index.js';
import { UmbMediaTypeTreeServerDataSource } from './media-type-tree.server.data-source.js';
import { UMB_MEDIA_TYPE_TREE_STORE_CONTEXT } from './media-type-tree.store.js';
import { UmbMediaTypeTreeItemModel, UmbMediaTypeTreeRootModel } from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaTypeTreeRepository
	extends UmbTreeRepositoryBase<UmbMediaTypeTreeItemModel, UmbMediaTypeTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTypeTreeServerDataSource, UMB_MEDIA_TYPE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			id: null,
			entityType: UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
			name: 'Media Types',
			icon: 'icon-folder',
			hasChildren: true,
			isContainer: false,
			isFolder: true,
		};

		return { data };
	}
}
