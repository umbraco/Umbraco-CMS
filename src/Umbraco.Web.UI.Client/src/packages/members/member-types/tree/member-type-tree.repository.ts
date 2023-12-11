import { UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMemberTypeTreeServerDataSource } from './member-type-tree.server.data-source.js';
import { UmbMemberTypeTreeItemModel, UmbMemberTypeTreeRootModel } from './types.js';
import { UMB_MEMBER_TYPE_TREE_STORE_CONTEXT } from './member-type-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMemberTypeTreeRepository
	extends UmbTreeRepositoryBase<UmbMemberTypeTreeItemModel, UmbMemberTypeTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberTypeTreeServerDataSource, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			id: null,
			type: UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE,
			name: 'Member Types',
			icon: 'icon-folder',
			hasChildren: true,
			isContainer: false,
			isFolder: true,
		};

		return { data };
	}
}
