import { UMB_RELATION_TYPE_ROOT_ENTITY_TYPE } from '../entities.js';
import { UmbRelationTypeTreeServerDataSource } from './relation-type-tree.server.data-source.js';
import { UmbRelationTypeTreeItemModel, UmbRelationTypeTreeRootModel } from './types.js';
import { UMB_RELATION_TYPE_TREE_STORE_CONTEXT } from './relation-type-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbRelationTypeTreeRepository
	extends UmbTreeRepositoryBase<UmbRelationTypeTreeItemModel, UmbRelationTypeTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbRelationTypeTreeServerDataSource, UMB_RELATION_TYPE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			id: null,
			type: UMB_RELATION_TYPE_ROOT_ENTITY_TYPE,
			name: 'Relation Types',
			icon: 'icon-folder',
			hasChildren: true,
		};

		return { data };
	}
}
