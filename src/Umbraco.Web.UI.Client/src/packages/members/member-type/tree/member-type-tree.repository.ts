import { UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMemberTypeTreeServerDataSource } from './server-data-source/member-type-tree.server.data-source.js';
import type { UmbMemberTypeTreeItemModel, UmbMemberTypeTreeRootModel } from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMemberTypeTreeRepository
	extends UmbTreeRepositoryBase<UmbMemberTypeTreeItemModel, UmbMemberTypeTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberTypeTreeServerDataSource);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ paging: { skip: 0, take: 0 } });
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
