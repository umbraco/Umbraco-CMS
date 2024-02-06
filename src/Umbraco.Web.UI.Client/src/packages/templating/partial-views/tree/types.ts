import type { UmbPartialViewEntityType, UmbPartialViewFolderEntityType, UmbPartialViewRootEntityType } from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbPartialViewTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbPartialViewEntityType | UmbPartialViewFolderEntityType;
}

export interface UmbPartialViewTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbPartialViewRootEntityType;
}
