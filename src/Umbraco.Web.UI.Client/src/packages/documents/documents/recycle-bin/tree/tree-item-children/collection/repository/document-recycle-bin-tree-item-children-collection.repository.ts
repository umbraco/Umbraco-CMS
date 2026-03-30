import { UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS } from '../../../constants.js';
import { UmbTreeItemChildrenCollectionRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentRecycleBinTreeItemChildrenCollectionRepository extends UmbTreeItemChildrenCollectionRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
		this._setTreeRepositoryAlias(UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS);
	}
}

export { UmbDocumentRecycleBinTreeItemChildrenCollectionRepository as api };
