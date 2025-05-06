import { UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS } from '../tree/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMenuTreeStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';

export class UmbPartialViewMenuStructureWorkspaceContext extends UmbMenuTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS });
	}
}

export default UmbPartialViewMenuStructureWorkspaceContext;
