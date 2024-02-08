import { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_TYPE_TREE_STORE_CONTEXT } from '../media-type-tree.store.js';
import { UmbMediaTypeFolderServerDataSource } from './media-type-folder.server.data-source.js';
import type { UmbMediaTypeFolderTreeItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';

export class UmbMediaTypeFolderRepository extends UmbFolderRepositoryBase<UmbMediaTypeFolderTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTypeFolderServerDataSource, UMB_MEDIA_TYPE_TREE_STORE_CONTEXT, folderToMediaTypeTreeItemMapper);
	}
}

const folderToMediaTypeTreeItemMapper = (folder: UmbFolderModel) => {
	const folderTreeItem: UmbMediaTypeFolderTreeItemModel = {
		unique: folder.unique,
		parentUnique: folder.parentUnique,
		name: folder.name,
		entityType: UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
		hasChildren: false,
		isFolder: true,
	};

	return folderTreeItem;
};
