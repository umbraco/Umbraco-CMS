import type { UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbMemberTypeTreeItemModel } from '../types.js';

export interface UmbMemberTypeFolderTreeItemModel extends UmbMemberTypeTreeItemModel {
	entityType: typeof UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE;
}
