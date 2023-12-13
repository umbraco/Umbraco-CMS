import { UmbTreeRepositoryBase } from '../../tree/tree-repository-base.js';
import { UMB_DATA_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDataTypeTreeServerDataSource } from './data-type-tree.server.data-source.js';
import { UMB_DATA_TYPE_TREE_STORE_CONTEXT } from './data-type-tree.store.js';
import { UmbDataTypeTreeItemModel, UmbDataTypeTreeRootModel } from './types.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDataTypeTreeRepository
	extends UmbTreeRepositoryBase<UmbDataTypeTreeItemModel, UmbDataTypeTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeTreeServerDataSource, UMB_DATA_TYPE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			unique: null,
			entityType: UMB_DATA_TYPE_ROOT_ENTITY_TYPE,
			name: 'Data Types',
			icon: 'icon-folder',
			hasChildren: true,
			isContainer: false,
			isFolder: true,
		};

		return { data };
	}
}
