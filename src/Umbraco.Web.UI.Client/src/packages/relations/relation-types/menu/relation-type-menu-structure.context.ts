import { UMB_RELATION_TYPE_TREE_REPOSITORY_ALIAS } from '../tree/index.js';
import { UmbMenuTreeStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbRelationTypeMenuStructureWorkspaceContext extends UmbMenuTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_RELATION_TYPE_TREE_REPOSITORY_ALIAS });
	}
}

export default UmbRelationTypeMenuStructureWorkspaceContext;
