import { UmbEntityTreeRepositoryBase } from '../../tree/entity-tree.repository.js';
import { DATA_TYPE_ROOT_ENTITY_TYPE } from '../entities.js';
import { UmbDataTypeTreeServerDataSource } from './data-type.tree.server.data.js';
import { UMB_DATA_TYPE_TREE_STORE_CONTEXT } from './data-type.tree.store.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DataTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';
export class UmbDataTypeTreeRepository
	extends UmbEntityTreeRepositoryBase<DataTypeTreeItemResponseModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeTreeServerDataSource, UMB_DATA_TYPE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			id: null,
			type: DATA_TYPE_ROOT_ENTITY_TYPE,
			name: 'Data Types',
			icon: 'icon-folder',
			hasChildren: true,
		};

		return { data };
	}
}
