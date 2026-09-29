import { UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS } from '../../../constants.js';
import { UmbTreeItemChildrenCollectionRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated Deprecated since v18. Scheduled for removal in Umbraco 20.
 */
export class UmbDocumentTypeTreeItemChildrenCollectionRepository extends UmbTreeItemChildrenCollectionRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
		new UmbDeprecation({
			deprecated: 'UmbDocumentTypeTreeItemChildrenCollectionRepository',
			removeInVersion: '20.0.0',
			solution: 'Use UmbDocumentTypeTreeRepository instead.',
		}).warn();
		this._setTreeRepositoryAlias(UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS);
	}
}

export { UmbDocumentTypeTreeItemChildrenCollectionRepository as api };
