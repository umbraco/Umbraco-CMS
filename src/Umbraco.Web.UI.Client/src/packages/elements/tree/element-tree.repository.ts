import { UMB_ELEMENT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbElementTreeServerDataSource } from './element.tree.server.data-source.js';
import type { UmbElementTreeItemModel, UmbElementTreeRootModel } from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementTreeRepository
	extends UmbTreeRepositoryBase<UmbElementTreeItemModel, UmbElementTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbElementTreeServerDataSource);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 0 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbElementTreeRootModel = {
			unique: null,
			entityType: UMB_ELEMENT_ROOT_ENTITY_TYPE,
			name: '#general_elements',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export { UmbElementTreeRepository as api };
