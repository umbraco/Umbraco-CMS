import { UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbTemplateTreeServerDataSource } from './template-tree.server.data-source.js';
import { UmbTemplateTreeItemModel, UmbTemplateTreeRootModel } from './types.js';
import { UMB_TEMPLATE_TREE_STORE_CONTEXT } from './template-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbTemplateTreeRepository
	extends UmbTreeRepositoryBase<UmbTemplateTreeItemModel, UmbTemplateTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbTemplateTreeServerDataSource, UMB_TEMPLATE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			id: null,
			type: UMB_TEMPLATE_ROOT_ENTITY_TYPE,
			name: 'Templates',
			icon: 'icon-folder',
			hasChildren: true,
			isContainer: false,
			isFolder: true,
		};

		return { data };
	}
}
