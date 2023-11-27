import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_DATA_TYPE_TREE_STORE_CONTEXT } from '../../tree/index.js';
import { UmbDataTypeFolderServerDataSource } from './data-type-folder.server.data-source.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbFolderModel, UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDataTypeFolderRepository extends UmbFolderRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeFolderServerDataSource, UMB_DATA_TYPE_TREE_STORE_CONTEXT, folderToDataTypeTreeItemFolder);
	}
}

const folderToDataTypeTreeItemFolder = (folder: UmbFolderModel) => {
	return {
		id: folder.unique,
		name: folder.name,
		type: UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
		hasChildren: false,
		isFolder: true,
	};
};
