import type { UmbTreeItemModel } from '../../types.js';
import type { ManifestTreeItemCard } from '../tree-item-card.extension.js';
import { UmbTreeItemApiContextBase } from '../../tree-item-api/tree-item-api-context-base.js';
import type { UmbTreeItemCardApi } from '../types.js';

export class UmbDefaultTreeItemCardApi<TreeItemType extends UmbTreeItemModel = UmbTreeItemModel>
	extends UmbTreeItemApiContextBase<TreeItemType, ManifestTreeItemCard>
	implements UmbTreeItemCardApi {}
