import { UmbTreeRepositoryBase } from '../../core/tree/tree-repository-base.js';
import { UMB_DATA_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDataTypeTreeServerDataSource } from './data-type-tree.server.data-source.js';
import { UMB_DATA_TYPE_TREE_STORE_CONTEXT } from './data-type-tree.store.js';
import type { UmbDataTypeTreeItemModel, UmbDataTypeTreeRootModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDataTypeTreeRepository
	extends UmbTreeRepositoryBase<UmbDataTypeTreeItemModel, UmbDataTypeTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeTreeServerDataSource, UMB_DATA_TYPE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data: UmbDataTypeTreeRootModel = {
			unique: null,
			entityType: UMB_DATA_TYPE_ROOT_ENTITY_TYPE,
			name: 'Data Types',
			hasChildren: true,
			isFolder: true,
		};

		return { data };
	}
}
