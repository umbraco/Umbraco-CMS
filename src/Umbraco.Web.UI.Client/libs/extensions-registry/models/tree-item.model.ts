import { UmbTreeItemElement } from '../interfaces';
import type { ManifestElement } from '.';

export interface ManifestTreeItem extends ManifestElement<UmbTreeItemElement> {
	type: 'treeItem';
	conditions: ConditionsTreeItem;
}

export interface ConditionsTreeItem {
	entityType: string;
}
