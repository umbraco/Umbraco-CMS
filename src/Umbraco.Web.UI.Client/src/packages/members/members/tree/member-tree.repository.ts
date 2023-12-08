import { UMB_MEMBER_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMemberTreeServerDataSource } from './member-tree.server.data-source.js';
import { UmbMemberTreeItemModel, UmbMemberTreeRootModel } from './types.js';
import { UMB_MEMBER_TREE_STORE_CONTEXT } from './member-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMemberTreeRepository
	extends UmbTreeRepositoryBase<UmbMemberTreeItemModel, UmbMemberTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberTreeServerDataSource, UMB_MEMBER_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			id: null,
			type: UMB_MEMBER_ROOT_ENTITY_TYPE,
			name: 'Members',
			icon: 'icon-folder',
			hasChildren: true,
			isContainer: false,
		};

		return { data };
	}
}
