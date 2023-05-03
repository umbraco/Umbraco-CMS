import { UmbTreeItemExtensionElement } from '../interfaces';
import type { ManifestElement } from '.';

export interface ManifestTreeItem extends ManifestElement<UmbTreeItemExtensionElement> {
	type: 'treeItem';
	conditions: ConditionsTreeItem;
}

export interface ConditionsTreeItem {
	entityTypes: Array<string>;
}
