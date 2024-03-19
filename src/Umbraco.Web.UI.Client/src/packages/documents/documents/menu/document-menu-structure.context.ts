import { UMB_DOCUMENT_TREE_REPOSITORY_ALIAS } from '../tree/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMenuVariantTreeStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';

export class UmbDocumentMenuStructureContext extends UmbMenuVariantTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_DOCUMENT_TREE_REPOSITORY_ALIAS });
	}
}

export default UmbDocumentMenuStructureContext;
