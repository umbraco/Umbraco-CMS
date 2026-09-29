import { UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS } from '../../../constants.js';
import { UmbTreeItemChildrenCollectionRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated Deprecated since v17. Scheduled for removal in Umbraco 20.
 */
export class UmbDocumentBlueprintTreeItemChildrenCollectionRepository extends UmbTreeItemChildrenCollectionRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
		new UmbDeprecation({
			deprecated: 'UmbDocumentBlueprintTreeItemChildrenCollectionRepository',
			removeInVersion: '20.0.0',
			solution: 'Use UmbDocumentBlueprintTreeRepository instead.',
		}).warn();
		this._setTreeRepositoryAlias(UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS);
	}
}

export { UmbDocumentBlueprintTreeItemChildrenCollectionRepository as api };
