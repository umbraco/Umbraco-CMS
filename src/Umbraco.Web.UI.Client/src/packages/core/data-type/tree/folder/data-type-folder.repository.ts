import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_DATA_TYPE_TREE_STORE_CONTEXT } from '../../tree/index.js';
import { UmbDataTypeFolderServerDataSource } from './data-type-folder.server.data-source.js';
import { UmbDataTypeFolderTreeItemModel } from './types.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbFolderModel, UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';

export class UmbDataTypeFolderRepository extends UmbFolderRepositoryBase<UmbDataTypeFolderTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeFolderServerDataSource, UMB_DATA_TYPE_TREE_STORE_CONTEXT, folderToDataTypeTreeItemMapper);
	}
}

const folderToDataTypeTreeItemMapper = (folder: UmbFolderModel) => {
	const folderTreeItem: UmbDataTypeFolderTreeItemModel = {
		unique: folder.unique,
		parentUnique: folder.parentUnique,
		name: folder.name,
		entityType: UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
		isContainer: false,
		hasChildren: false,
		isFolder: true,
	};

	return folderTreeItem;
};
