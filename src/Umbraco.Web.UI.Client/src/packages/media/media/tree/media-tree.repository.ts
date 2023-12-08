import { UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMediaTreeServerDataSource } from './media-tree.server.data-source.js';
import { UmbMediaTreeItemModel, UmbMediaTreeRootModel } from './types.js';
import { UMB_MEDIA_TREE_STORE_CONTEXT } from './media-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaTreeRepository
	extends UmbTreeRepositoryBase<UmbMediaTreeItemModel, UmbMediaTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTreeServerDataSource, UMB_MEDIA_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			id: null,
			type: UMB_MEDIA_ROOT_ENTITY_TYPE,
			name: 'Medias',
			icon: 'icon-folder',
			hasChildren: true,
			isContainer: false,
		};

		return { data };
	}
}
