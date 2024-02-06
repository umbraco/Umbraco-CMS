import type { UmbStylesheetEntityType, UmbStylesheetFolderEntityType, UmbStylesheetRootEntityType } from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbStylesheetTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbStylesheetEntityType | UmbStylesheetFolderEntityType;
}

export interface UmbStylesheetTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbStylesheetRootEntityType;
}
