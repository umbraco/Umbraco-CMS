import { UmbMediaTypeFolderRepository } from './media-type-folder.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.MediaType.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
	name: 'Media Type Folder Repository',
	api: UmbMediaTypeFolderRepository,
};

export const manifests = [folderRepository];
