import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT } from '../document-type.tree.store.js';
import { UmbDocumentTypeFolderServerDataSource } from './document-type-folder.server.data-source.js';
import type { UmbDocumentTypeFolderTreeItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';

export class UmbDocumentTypeFolderRepository extends UmbFolderRepositoryBase<UmbDocumentTypeFolderTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(
			host,
			UmbDocumentTypeFolderServerDataSource,
			UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT,
			folderToDocumentTypeTreeItemMapper,
		);
	}
}

const folderToDocumentTypeTreeItemMapper = (folder: UmbFolderModel): UmbDocumentTypeFolderTreeItemModel => {
	return {
		unique: folder.unique,
		parentUnique: folder.parentUnique,
		name: folder.name,
		entityType: UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
		hasChildren: false,
		isFolder: true,
		isElement: false,
	};
};
