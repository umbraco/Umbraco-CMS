import { UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_SCRIPT_TREE_STORE_CONTEXT, UmbScriptTreeItemModel } from '../index.js';
import { UmbScriptFolderServerDataSource } from './script-folder.server.data-source.js';
import { UmbFolderModel, UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbScriptFolderRepository extends UmbFolderRepositoryBase<UmbScriptTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbScriptFolderServerDataSource, UMB_SCRIPT_TREE_STORE_CONTEXT, folderToScriptTreeItemFolder);
	}
}

const folderToScriptTreeItemFolder = (folder: UmbFolderModel) => {
	const treeItem: UmbScriptTreeItemModel = {
		unique: folder.unique,
		parentUnique: folder.parentUnique,
		name: folder.name,
		entityType: UMB_SCRIPT_FOLDER_ENTITY_TYPE,
		isFolder: true,
		isContainer: false,
		hasChildren: false,
	};

	return treeItem;
};
