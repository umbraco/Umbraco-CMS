import type { UmbTreeItemModel } from '../../types.js';
import { UmbTreeItemApiBase } from '../../tree-item/tree-item-base/tree-item-api-base.js';

export class UmbDefaultTreeItemTableApi<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
> extends UmbTreeItemApiBase<TreeItemType> {}
