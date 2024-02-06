import { UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbTemplateTreeServerDataSource } from './template-tree.server.data-source.js';
import type { UmbTemplateTreeItemModel, UmbTemplateTreeRootModel } from './types.js';
import { UMB_TEMPLATE_TREE_STORE_CONTEXT } from './template-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbTemplateTreeRepository
	extends UmbTreeRepositoryBase<UmbTemplateTreeItemModel, UmbTemplateTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbTemplateTreeServerDataSource, UMB_TEMPLATE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data: UmbTemplateTreeRootModel = {
			id: null,
			entityType: UMB_TEMPLATE_ROOT_ENTITY_TYPE,
			name: 'Templates',
			hasChildren: true,
			isFolder: true,
		};

		return { data };
	}
}
