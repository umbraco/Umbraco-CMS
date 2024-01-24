import { UmbDataTypeEntityType, UmbDataTypeFolderEntityType, UmbDataTypeRootEntityType } from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDataTypeTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbDataTypeEntityType | UmbDataTypeFolderEntityType;
}

export interface UmbDataTypeTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbDataTypeRootEntityType;
}
