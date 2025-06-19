import type { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbMediaTypeTreeItemModel } from '../types.js';

export interface UmbMediaTypeFolderTreeItemModel extends UmbMediaTypeTreeItemModel {
	entityType: typeof UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE;
}
