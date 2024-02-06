import type { UmbStaticFileEntityType, UmbStaticFileFolderEntityType, UmbStaticFileRootEntityType } from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbStaticFileTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbStaticFileEntityType | UmbStaticFileFolderEntityType;
}

export interface UmbStaticFileTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbStaticFileRootEntityType;
}
