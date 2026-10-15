import { UMB_STYLESHEET_TREE_REPOSITORY_ALIAS } from '../../../constants.js';
import { UmbTreeItemChildrenCollectionRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated Deprecated since v17. Scheduled for removal in Umbraco 20.
 */
export class UmbStylesheetTreeItemChildrenCollectionRepository extends UmbTreeItemChildrenCollectionRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
		new UmbDeprecation({
			deprecated: 'UmbStylesheetTreeItemChildrenCollectionRepository',
			removeInVersion: '20.0.0',
			solution: 'Use UmbStylesheetTreeRepository instead.',
		}).warn();
		this._setTreeRepositoryAlias(UMB_STYLESHEET_TREE_REPOSITORY_ALIAS);
	}
}

export { UmbStylesheetTreeItemChildrenCollectionRepository as api };
