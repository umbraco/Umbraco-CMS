import { UMB_STATIC_FILE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbStaticFileTreeServerDataSource } from './static-file-tree.server.data-source.js';
import { UmbStaticFileTreeItemModel, UmbStaticFileTreeRootModel } from './types.js';
import { UMB_STATIC_FILE_TREE_STORE_CONTEXT } from './static-file-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbStaticFileTreeRepository
	extends UmbTreeRepositoryBase<UmbStaticFileTreeItemModel, UmbStaticFileTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbStaticFileTreeServerDataSource, UMB_STATIC_FILE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			path: null,
			entityType: UMB_STATIC_FILE_ROOT_ENTITY_TYPE,
			name: 'StaticFiles',
			icon: 'icon-folder',
			hasChildren: true,
			isContainer: false,
			isFolder: true,
		};

		return { data };
	}
}
