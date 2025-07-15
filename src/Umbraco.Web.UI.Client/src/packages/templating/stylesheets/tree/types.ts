import type { UmbStylesheetEntityType, UmbStylesheetFolderEntityType, UmbStylesheetRootEntityType } from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbStylesheetTreeItemModel extends UmbTreeItemModel {
	entityType: UmbStylesheetEntityType | UmbStylesheetFolderEntityType;
}

export interface UmbStylesheetTreeRootModel extends UmbTreeRootModel {
	entityType: UmbStylesheetRootEntityType;
}
