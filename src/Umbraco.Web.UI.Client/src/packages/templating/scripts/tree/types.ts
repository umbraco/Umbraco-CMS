import type { UmbScriptEntityType, UmbScriptFolderEntityType, UmbScriptRootEntityType } from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbScriptTreeItemModel extends UmbTreeItemModel {
	entityType: UmbScriptEntityType | UmbScriptFolderEntityType;
}

export interface UmbScriptTreeRootModel extends UmbTreeRootModel {
	entityType: UmbScriptRootEntityType;
}
