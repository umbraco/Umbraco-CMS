import { UmbTreeItemExtensionElement } from '../interfaces';
import type { ManifestElement } from 'src/libs/extension-api';

export interface ManifestTreeItem extends ManifestElement<UmbTreeItemExtensionElement> {
	type: 'treeItem';
	conditions: ConditionsTreeItem;
}

export interface ConditionsTreeItem {
	entityTypes: Array<string>;
}
