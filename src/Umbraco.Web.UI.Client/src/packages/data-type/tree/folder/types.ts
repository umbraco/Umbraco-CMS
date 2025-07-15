import type { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbDataTypeTreeItemModel } from '../types.js';

export interface UmbDataTypeFolderTreeItemModel extends UmbDataTypeTreeItemModel {
	entityType: typeof UMB_DATA_TYPE_FOLDER_ENTITY_TYPE;
}
