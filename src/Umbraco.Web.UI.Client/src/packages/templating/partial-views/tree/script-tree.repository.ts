import { UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbPartialViewTreeServerDataSource } from './script-tree.server.data-source.js';
import { UmbPartialViewTreeItemModel, UmbPartialViewTreeRootModel } from './types.js';
import { UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT } from './script-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbPartialViewTreeRepository
	extends UmbTreeRepositoryBase<UmbPartialViewTreeItemModel, UmbPartialViewTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbPartialViewTreeServerDataSource, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			path: null,
			type: UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
			name: 'Partial Views',
			icon: 'icon-folder',
			hasChildren: true,
		};

		return { data };
	}
}
