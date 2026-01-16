import { UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../constants.js';
import type { UmbElementRecycleBinTreeItemModel, UmbElementRecycleBinTreeRootModel } from '../types.js';
import { UmbElementRecycleBinTreeServerDataSource } from './element-recycle-bin-tree.server.data-source.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbElementRecycleBinTreeRepository
	extends UmbTreeRepositoryBase<UmbElementRecycleBinTreeItemModel, UmbElementRecycleBinTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbElementRecycleBinTreeServerDataSource);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 0 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data = {
			unique: null,
			entityType: UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_contentRecycleBin',
			icon: 'icon-trash',
			hasChildren,
			isContainer: false,
			isFolder: true,
		};

		return { data };
	}
}

export { UmbElementRecycleBinTreeRepository as api };
