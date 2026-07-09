import { UMB_SCRIPT_TREE_REPOSITORY_ALIAS } from '../../../constants.js';
import { UmbTreeItemChildrenCollectionRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbScriptTreeItemChildrenCollectionRepository extends UmbTreeItemChildrenCollectionRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
		this._setTreeRepositoryAlias(UMB_SCRIPT_TREE_REPOSITORY_ALIAS);
	}
}

export { UmbScriptTreeItemChildrenCollectionRepository as api };
