import { UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../entities.js';
import { UmbScriptTreeServerDataSource } from './script-tree.server.data-source.js';
import { UmbScriptTreeItemModel, UmbScriptTreeRootModel } from './types.js';
import { UMB_SCRIPT_TREE_STORE_CONTEXT } from './script-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbScriptTreeRepository
	extends UmbTreeRepositoryBase<UmbScriptTreeItemModel, UmbScriptTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbScriptTreeServerDataSource, UMB_SCRIPT_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			id: null,
			type: UMB_SCRIPT_ROOT_ENTITY_TYPE,
			name: 'Scripts',
			icon: 'icon-folder',
			hasChildren: true,
		};

		return { data };
	}
}
