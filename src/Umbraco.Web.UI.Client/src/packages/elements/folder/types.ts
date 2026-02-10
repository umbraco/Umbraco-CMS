import type { UmbElementTreeItemModel } from '../tree/types.js';
import type { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../entity.js';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export type * from './repository/types.js';

export interface UmbElementFolderTreeItemModel extends UmbElementTreeItemModel {
	entityType: typeof UMB_ELEMENT_FOLDER_ENTITY_TYPE;
}

export interface UmbElementFolderModel extends UmbFolderModel {
	isTrashed?: boolean;
}
