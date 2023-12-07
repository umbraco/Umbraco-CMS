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
		// TODO: TREE STORE TYPE PROBLEM:
		// TODO: This is wrong, either this should be adapted to use unique, or ID. If so the types above should be updated and the UmbTreeRepositoryBase should also be updated to use unique.
		const data = {
			id: null,
			unique: null,
			type: UMB_DATA_TYPE_ROOT_ENTITY_TYPE,
			name: 'Data Types',
			icon: 'icon-folder',
			hasChildren: true,
		};

		return { data };
	}
}
