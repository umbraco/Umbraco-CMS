import { UMB_SCRIPT_TREE_REPOSITORY_ALIAS } from '../../../constants.js';
import { UmbTreeItemChildrenCollectionRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated Deprecated since v17. Scheduled for removal in Umbraco 20.
 */
export class UmbScriptTreeItemChildrenCollectionRepository extends UmbTreeItemChildrenCollectionRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
		new UmbDeprecation({
			deprecated: 'UmbScriptTreeItemChildrenCollectionRepository',
			removeInVersion: '20.0.0',
			solution: 'Use UmbScriptTreeRepository instead.',
		}).warn();
		this._setTreeRepositoryAlias(UMB_SCRIPT_TREE_REPOSITORY_ALIAS);
	}
}

export { UmbScriptTreeItemChildrenCollectionRepository as api };
