import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbTreeItemModelBase } from '../../types.js';

export interface UmbMoveSelectableFilterProvider<TreeItemType extends UmbTreeItemModelBase = UmbTreeItemModelBase>
	extends UmbApi {
	/**
	 * Get the selectable filter for the move action.
	 * @param contentTypeUnique - The content type ID of the item being moved
	 * @returns A filter function that determines if a tree item is selectable
	 */
	getSelectableFilter(contentTypeUnique: string): Promise<(item: TreeItemType) => boolean>;
}
