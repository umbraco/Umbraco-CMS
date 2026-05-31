import { UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMediaTreeServerDataSource } from './server-data-source/media-tree.server.data-source.js';
import type {
	UmbMediaTreeChildrenOfRequestArgs,
	UmbMediaTreeItemModel,
	UmbMediaTreeRootItemsRequestArgs,
	UmbMediaTreeRootModel,
} from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaTreeRepository
	extends UmbTreeRepositoryBase<
		UmbMediaTreeItemModel,
		UmbMediaTreeRootModel,
		UmbMediaTreeRootItemsRequestArgs,
		UmbMediaTreeChildrenOfRequestArgs
	>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTreeServerDataSource);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ paging: { skip: 0, take: 0 } });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbMediaTreeRootModel = {
			unique: null,
			entityType: UMB_MEDIA_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_media',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export default UmbMediaTreeRepository;
