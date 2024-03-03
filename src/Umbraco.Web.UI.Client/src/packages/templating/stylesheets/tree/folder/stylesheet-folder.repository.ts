import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbStylesheetTreeItemModel } from '../types.js';
import { UMB_STYLESHEET_TREE_STORE_CONTEXT } from '../stylesheet-tree.store.js';
import { UmbStylesheetFolderServerDataSource } from './stylesheet-folder.server.data-source.js';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbStylesheetFolderRepository extends UmbFolderRepositoryBase<UmbStylesheetTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(
			host,
			UmbStylesheetFolderServerDataSource,
			UMB_STYLESHEET_TREE_STORE_CONTEXT,
			folderToStylesheetTreeItemFolder,
		);
	}
}

export default UmbStylesheetFolderRepository;

const folderToStylesheetTreeItemFolder = (
	folder: UmbFolderModel,
	parentUnique: string | null,
): UmbStylesheetTreeItemModel => {
	return {
		unique: folder.unique,
		parentUnique,
		name: folder.name,
		entityType: UMB_STYLESHEET_FOLDER_ENTITY_TYPE,
		isFolder: true,
		hasChildren: false,
	};
};
