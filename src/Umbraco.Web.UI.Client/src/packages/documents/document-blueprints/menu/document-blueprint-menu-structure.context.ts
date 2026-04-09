import { UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS } from '../tree/index.js';
import { UmbMenuTreeStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';

import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentBlueprintMenuStructureWorkspaceContext extends UmbMenuTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS });
	}
}

export { UmbDocumentBlueprintMenuStructureWorkspaceContext as api };
