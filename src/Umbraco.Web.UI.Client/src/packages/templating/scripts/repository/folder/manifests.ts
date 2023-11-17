import { UmbScriptFolderRepository } from './script-folder.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.Script.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
	name: 'Script Folder Repository',
	api: UmbScriptFolderRepository,
};

export const manifests = [folderRepository];
