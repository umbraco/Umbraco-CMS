import { UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../constants.js';
import { UmbMediaRecycleBinTreeServerDataSource } from './media-recycle-bin-tree.server.data-source.js';
import type { UmbMediaRecycleBinTreeItemModel, UmbMediaRecycleBinTreeRootModel } from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaRecycleBinTreeRepository
	extends UmbTreeRepositoryBase<UmbMediaRecycleBinTreeItemModel, UmbMediaRecycleBinTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaRecycleBinTreeServerDataSource);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ paging: { skip: 0, take: 0 } });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data = {
			unique: null,
			entityType: UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_contentRecycleBin',
			icon: 'icon-trash',
			hasChildren,
			isContainer: false,
			isFolder: true,
		};

		return { data };
	}
}

export { UmbMediaRecycleBinTreeRepository as api };
