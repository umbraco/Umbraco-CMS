import { UMB_STYLESHEET_TREE_REPOSITORY_ALIAS } from '../constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMenuTreeStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';

export class UmbStylesheetMenuStructureWorkspaceContext extends UmbMenuTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_STYLESHEET_TREE_REPOSITORY_ALIAS });
	}
}

export default UmbStylesheetMenuStructureWorkspaceContext;
