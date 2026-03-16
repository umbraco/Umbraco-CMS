import { UMB_MEDIA_RECYCLE_BIN_TREE_REPOSITORY_ALIAS } from '../../../constants.js';
import { UmbTreeItemChildrenCollectionRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaRecycleBinTreeItemChildrenCollectionRepository extends UmbTreeItemChildrenCollectionRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
		this._setTreeRepositoryAlias(UMB_MEDIA_RECYCLE_BIN_TREE_REPOSITORY_ALIAS);
	}
}

export { UmbMediaRecycleBinTreeItemChildrenCollectionRepository as api };
