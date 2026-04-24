import type { UmbTreeItemModel } from '../../types.js';
import type { ManifestTreeItemCard } from '../tree-item-card.extension.js';
import { UmbTreeItemApiBase } from '../../tree-item/tree-item-base/tree-item-api-base.js';

export class UmbDefaultTreeItemCardApi<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
> extends UmbTreeItemApiBase<TreeItemType, ManifestTreeItemCard> {}

export default UmbDefaultTreeItemCardApi;
