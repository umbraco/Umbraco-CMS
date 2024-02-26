import { UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMemberTypeTreeServerDataSource } from './member-type-tree.server.data-source.js';
import type { UmbMemberTypeTreeItemModel, UmbMemberTypeTreeRootModel } from './types.js';
import { UMB_MEMBER_TYPE_TREE_STORE_CONTEXT } from './member-type-tree.store.js';
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
		const data: UmbMemberTypeTreeRootModel = {
			unique: null,
			entityType: UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE,
			name: 'Member Types',
			hasChildren: true,
			isFolder: true,
		};

		return { data };
	}
}

export default UmbMemberTypeTreeRepository;
