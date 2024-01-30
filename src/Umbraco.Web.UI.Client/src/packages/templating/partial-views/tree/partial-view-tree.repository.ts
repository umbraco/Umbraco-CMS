import { UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbPartialViewTreeServerDataSource } from './partial-view-tree.server.data-source.js';
import type { UmbPartialViewTreeItemModel, UmbPartialViewTreeRootModel } from './types.js';
import { UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT } from './partial-view-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbPartialViewTreeRepository
	extends UmbTreeRepositoryBase<UmbPartialViewTreeItemModel, UmbPartialViewTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbPartialViewTreeServerDataSource, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data: UmbPartialViewTreeRootModel = {
			unique: null,
			entityType: UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
			name: 'Partial Views',
			hasChildren: true,
			isFolder: true,
		};

		return { data };
	}
}
