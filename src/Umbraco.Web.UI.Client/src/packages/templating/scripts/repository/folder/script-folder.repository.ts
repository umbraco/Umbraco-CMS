import { UMB_SCRIPT_TREE_STORE_CONTEXT } from '../../tree/index.js';
import { UmbScriptFolderServerDataSource } from './script-folder.server.data-source.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbFolderModel, UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbScriptFolderRepository extends UmbFolderRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbScriptFolderServerDataSource, UMB_SCRIPT_TREE_STORE_CONTEXT, folderToScriptTreeItemFolder);
	}
}

// TODO: Update when uniques are implemented everywhere
const folderToScriptTreeItemFolder = (folder: UmbFolderModel): FileSystemTreeItemPresentationModel => {
	return {
		path: folder.unique,
		name: folder.name,
		type: UMB_SCRIPT_FOLDER_ENTITY_TYPE,
		hasChildren: false,
		isFolder: true,
	};
};
