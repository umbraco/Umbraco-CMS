import { UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMemberTypeTreeServerDataSource } from './member-type-tree.server.data-source.js';
import type { UmbMemberTypeTreeItemModel, UmbMemberTypeTreeRootModel } from './types.js';
import { UMB_MEMBER_TYPE_TREE_STORE_CONTEXT } from './member-type-tree.store.context-token.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMemberTypeTreeRepository
	extends UmbTreeRepositoryBase<UmbMemberTypeTreeItemModel, UmbMemberTypeTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberTypeTreeServerDataSource, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 1 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbMemberTypeTreeRootModel = {
			unique: null,
			entityType: UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_memberTypes',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export default UmbMemberTypeTreeRepository;
