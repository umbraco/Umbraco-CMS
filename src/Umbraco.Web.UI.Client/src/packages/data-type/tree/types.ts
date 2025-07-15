import type { UmbDataTypeEntityType, UmbDataTypeFolderEntityType, UmbDataTypeRootEntityType } from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDataTypeTreeItemModel extends UmbTreeItemModel {
	entityType: UmbDataTypeEntityType | UmbDataTypeFolderEntityType;
}

export interface UmbDataTypeTreeRootModel extends UmbTreeRootModel {
	entityType: UmbDataTypeRootEntityType;
}
