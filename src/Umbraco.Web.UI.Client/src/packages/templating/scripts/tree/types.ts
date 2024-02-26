import type { UmbScriptEntityType, UmbScriptFolderEntityType, UmbScriptRootEntityType } from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbScriptTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbScriptEntityType | UmbScriptFolderEntityType;
}

export interface UmbScriptTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbScriptRootEntityType;
}
