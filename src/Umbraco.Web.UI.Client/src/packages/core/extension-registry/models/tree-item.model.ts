import type { UmbTreeItemModel } from '../../tree/types.js';
import type { UmbTreeItemContext } from '../../tree/tree-item/index.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTreeItem
	extends ManifestElementAndApi<UmbControllerHostElement, UmbTreeItemContext<UmbTreeItemModel>> {
	type: 'treeItem';
	forEntityTypes: Array<string>;
}
