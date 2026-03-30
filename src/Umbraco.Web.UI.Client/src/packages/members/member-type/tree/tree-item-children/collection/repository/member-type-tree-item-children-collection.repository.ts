import { UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS } from '../../../constants.js';
import { UmbTreeItemChildrenCollectionRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberTypeTreeItemChildrenCollectionRepository extends UmbTreeItemChildrenCollectionRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
		this._setTreeRepositoryAlias(UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS);
	}
}

export { UmbMemberTypeTreeItemChildrenCollectionRepository as api };
