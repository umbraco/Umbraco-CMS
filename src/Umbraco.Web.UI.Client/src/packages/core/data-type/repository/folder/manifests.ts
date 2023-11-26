import { UmbDataTypeFolderRepository } from './data-type-folder.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
	name: 'Data Type Folder Repository',
	api: UmbDataTypeFolderRepository,
};

export const manifests = [folderRepository];
