import { UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS } from '../tree/constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMenuVariantTreeStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';

export class UmbDocumentRecycleBinMenuStructureContext extends UmbMenuVariantTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS });
	}
}

export { UmbDocumentRecycleBinMenuStructureContext as api };
