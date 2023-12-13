import { UmbScriptEntityType, UmbScriptFolderEntityType, UmbScriptRootEntityType } from '../entity.js';
import type { UmbFileSystemTreeItemModel, UmbFileSystemTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbScriptTreeItemModel extends UmbFileSystemTreeItemModel {
	entityType: UmbScriptEntityType | UmbScriptFolderEntityType;
}

export interface UmbScriptTreeRootModel extends UmbFileSystemTreeRootModel {
	entityType: UmbScriptRootEntityType;
}
