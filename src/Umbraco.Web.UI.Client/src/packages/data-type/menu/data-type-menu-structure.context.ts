import { UMB_DATA_TYPE_TREE_REPOSITORY_ALIAS } from '../constants.js';
import { UmbMenuTreeStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';

import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDataTypeMenuStructureWorkspaceContext extends UmbMenuTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_DATA_TYPE_TREE_REPOSITORY_ALIAS });
	}
}

export default UmbDataTypeMenuStructureWorkspaceContext;
