import { UmbTreeItemExtensionElement } from '../interfaces/index.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTreeItem extends ManifestElement<UmbTreeItemExtensionElement> {
	type: 'treeItem';
	meta: MetaTreeItem;
}

export interface MetaTreeItem {
	entityTypes: Array<string>;
}
