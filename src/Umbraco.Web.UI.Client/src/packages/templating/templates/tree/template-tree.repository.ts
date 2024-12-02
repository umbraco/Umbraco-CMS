import { UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbTemplateTreeServerDataSource } from './template-tree.server.data-source.js';
import type { UmbTemplateTreeItemModel, UmbTemplateTreeRootModel } from './types.js';
import { UMB_TEMPLATE_TREE_STORE_CONTEXT } from './template-tree.store.context-token.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';

export class UmbTemplateTreeRepository
	extends UmbTreeRepositoryBase<UmbTemplateTreeItemModel, UmbTemplateTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbTemplateTreeServerDataSource, UMB_TEMPLATE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 1 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbTemplateTreeRootModel = {
			unique: null,
			entityType: UMB_TEMPLATE_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_templates',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export default UmbTemplateTreeRepository;
