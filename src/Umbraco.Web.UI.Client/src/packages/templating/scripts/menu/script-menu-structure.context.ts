import { UMB_SCRIPT_TREE_REPOSITORY_ALIAS } from '../tree/index.js';
import { UmbMenuStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';

import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbScriptMenuStructureWorkspaceContext extends UmbMenuStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_SCRIPT_TREE_REPOSITORY_ALIAS });
	}
}

export default UmbScriptMenuStructureWorkspaceContext;
