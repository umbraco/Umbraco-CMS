import { UMB_MEDIA_TREE_REPOSITORY_ALIAS } from '../constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMenuVariantTreeStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';

export class UmbMediaMenuStructureContext extends UmbMenuVariantTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_MEDIA_TREE_REPOSITORY_ALIAS });
	}
}

export default UmbMediaMenuStructureContext;
