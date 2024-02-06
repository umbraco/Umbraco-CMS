import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_DATA_TYPE_TREE_STORE_CONTEXT } from '../../tree/index.js';
import { UmbDataTypeFolderServerDataSource } from './data-type-folder.server.data-source.js';
import type { UmbDataTypeFolderTreeItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';

export class UmbDataTypeFolderRepository extends UmbFolderRepositoryBase<UmbDataTypeFolderTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeFolderServerDataSource, UMB_DATA_TYPE_TREE_STORE_CONTEXT, folderToDataTypeTreeItemMapper);
	}
}

const folderToDataTypeTreeItemMapper = (folder: UmbFolderModel): UmbDataTypeFolderTreeItemModel => {
	return {
		unique: folder.unique,
		parentUnique: folder.parentUnique,
		name: folder.name,
		entityType: UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
		hasChildren: false,
		isFolder: true,
	};
};
