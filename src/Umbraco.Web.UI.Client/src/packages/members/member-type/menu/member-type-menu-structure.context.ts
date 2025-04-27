import { UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS } from '../constants.js';
import { UmbMenuStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberTypeMenuStructureWorkspaceContext extends UmbMenuStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS });
	}
}

export default UmbMemberTypeMenuStructureWorkspaceContext;
