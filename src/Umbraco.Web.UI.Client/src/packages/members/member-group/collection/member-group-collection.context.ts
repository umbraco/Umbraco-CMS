import type { UmbMemberGroupCollectionItemModel } from './types.js';
import { UMB_EDIT_MEMBER_GROUP_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';

export class UmbMemberGroupCollectionContext extends UmbDefaultCollectionContext<UmbMemberGroupCollectionItemModel> {
	override async requestItemHref(item: UmbMemberGroupCollectionItemModel): Promise<string | undefined> {
		return UMB_EDIT_MEMBER_GROUP_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique });
	}
}

export { UmbMemberGroupCollectionContext as api };
