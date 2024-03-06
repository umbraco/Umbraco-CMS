import type { UmbTreeItemContext, UmbTreeItemModelBase } from '../../index.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTreeItem
	extends ManifestElementAndApi<UmbControllerHostElement, UmbTreeItemContext<UmbTreeItemModelBase>> {
	type: 'treeItem';
	forEntityTypes: Array<string>;
}
