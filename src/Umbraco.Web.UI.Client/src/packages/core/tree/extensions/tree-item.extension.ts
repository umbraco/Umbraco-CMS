import type { UmbTreeItemModel } from '../types.js';
import type { UmbTreeItemContext } from '../tree-item/index.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTreeItem
	extends ManifestElementAndApi<UmbControllerHostElement, UmbTreeItemContext<UmbTreeItemModel>> {
	type: 'treeItem';
	forEntityTypes: Array<string>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTreeItem: ManifestTreeItem;
	}
}
