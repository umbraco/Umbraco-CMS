import type { UmbElementTreeItemModel } from '../tree/types.js';
import type { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../entity.js';

export interface UmbElementFolderTreeItemModel extends UmbElementTreeItemModel {
	entityType: typeof UMB_ELEMENT_FOLDER_ENTITY_TYPE;
}
