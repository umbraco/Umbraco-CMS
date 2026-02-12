import { UMB_MEDIA_RECYCLE_BIN_TREE_REPOSITORY_ALIAS } from '../tree/constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMenuVariantTreeStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';

export class UmbMediaRecycleBinMenuStructureContext extends UmbMenuVariantTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_MEDIA_RECYCLE_BIN_TREE_REPOSITORY_ALIAS });
	}
}

export { UmbMediaRecycleBinMenuStructureContext as api };
