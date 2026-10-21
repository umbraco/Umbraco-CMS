import { UMB_ELEMENT_TREE_REPOSITORY_ALIAS } from '../tree/constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMenuVariantTreeStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';

export class UmbElementMenuStructureContext extends UmbMenuVariantTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_ELEMENT_TREE_REPOSITORY_ALIAS });
	}
}

export default UmbElementMenuStructureContext;
