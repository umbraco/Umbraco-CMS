import type { UmbElementEntityType, UmbElementRootEntityType } from '../entity.js';
import type { UmbElementFolderEntityType } from './folder/entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbElementTreeItemModel extends UmbTreeItemModel {
	entityType: UmbElementEntityType | UmbElementFolderEntityType;
}

export interface UmbElementTreeRootModel extends UmbTreeRootModel {
	entityType: UmbElementRootEntityType;
}
