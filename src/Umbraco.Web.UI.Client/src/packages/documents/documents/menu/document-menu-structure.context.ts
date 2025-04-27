import { UMB_DOCUMENT_TREE_REPOSITORY_ALIAS } from '../tree/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantMenuStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';

export class UmbDocumentMenuStructureContext extends UmbVariantMenuStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_DOCUMENT_TREE_REPOSITORY_ALIAS });
	}
}

export default UmbDocumentMenuStructureContext;
