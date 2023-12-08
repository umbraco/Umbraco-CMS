import { UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMemberGroupTreeServerDataSource } from './member-group-tree.server.data-source.js';
import { UmbMemberGroupTreeItemModel, UmbMemberGroupTreeRootModel } from './types.js';
import { UMB_MEMBER_GROUP_TREE_STORE_CONTEXT } from './member-group-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMemberGroupTreeRepository
	extends UmbTreeRepositoryBase<UmbMemberGroupTreeItemModel, UmbMemberGroupTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberGroupTreeServerDataSource, UMB_MEMBER_GROUP_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			id: null,
			type: UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE,
			name: 'Member Groups',
			icon: 'icon-folder',
			hasChildren: true,
			isContainer: false,
		};

		return { data };
	}
}
