import { UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_SCRIPT_TREE_STORE_CONTEXT } from '../script-tree.store.js';
import type { UmbScriptTreeItemModel } from '../types.js';
import { UmbScriptFolderServerDataSource } from './script-folder.server.data-source.js';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbScriptFolderRepository extends UmbFolderRepositoryBase<UmbScriptTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbScriptFolderServerDataSource, UMB_SCRIPT_TREE_STORE_CONTEXT, folderToScriptTreeItemFolder);
	}
}

const folderToScriptTreeItemFolder = (folder: UmbFolderModel): UmbScriptTreeItemModel => {
	return {
		unique: folder.unique,
		parentUnique: folder.parentUnique,
		name: folder.name,
		entityType: UMB_SCRIPT_FOLDER_ENTITY_TYPE,
		isFolder: true,
		hasChildren: false,
	};
};
