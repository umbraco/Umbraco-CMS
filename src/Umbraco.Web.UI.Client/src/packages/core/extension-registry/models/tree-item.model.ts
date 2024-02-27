import type { UmbTreeItemContext, UmbTreeItemModelBase } from '../../index.js';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTreeItem extends ManifestElementAndApi<HTMLElement, UmbTreeItemContext<UmbTreeItemModelBase>> {
	type: 'treeItem';
	meta: MetaTreeItem;
}

export interface MetaTreeItem {
	entityTypes: Array<string>;
}
