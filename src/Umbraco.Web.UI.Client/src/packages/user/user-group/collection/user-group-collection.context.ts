import { UMB_EDIT_USER_GROUP_WORKSPACE_PATH_PATTERN } from '../paths.js';
import type { UmbUserGroupDetailModel } from '../types.js';
import type { UmbUserGroupCollectionFilterModel } from './types.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';

export class UmbUserGroupCollectionContext extends UmbDefaultCollectionContext<
	UmbUserGroupDetailModel,
	UmbUserGroupCollectionFilterModel
> {
	/**
	 * Returns the href for a specific User Group collection item.
	 * @param {UmbUserGroupDetailModel} item - The user group item to get the href for.
	 * @returns {Promise<string | undefined>} - The edit workspace href for the user group.
	 */
	override async requestItemHref(item: UmbUserGroupDetailModel): Promise<string | undefined> {
		return `${UMB_EDIT_USER_GROUP_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique })}`;
	}
}

export { UmbUserGroupCollectionContext as api };
