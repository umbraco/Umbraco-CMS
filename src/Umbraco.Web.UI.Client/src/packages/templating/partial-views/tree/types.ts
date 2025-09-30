import type {
	UmbPartialViewEntityType,
	UmbPartialViewFolderEntityType,
	UmbPartialViewRootEntityType,
} from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbPartialViewTreeItemModel extends UmbTreeItemModel {
	entityType: UmbPartialViewEntityType | UmbPartialViewFolderEntityType;
}

export interface UmbPartialViewTreeRootModel extends UmbTreeRootModel {
	entityType: UmbPartialViewRootEntityType;
}
