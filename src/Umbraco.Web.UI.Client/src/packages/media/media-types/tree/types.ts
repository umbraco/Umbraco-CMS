import type { UmbMediaTypeEntityType, UmbMediaTypeFolderEntityType, UmbMediaTypeRootEntityType } from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbMediaTypeTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbMediaTypeEntityType | UmbMediaTypeFolderEntityType;
}

export interface UmbMediaTypeTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbMediaTypeRootEntityType;
}
