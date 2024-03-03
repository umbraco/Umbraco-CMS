import { UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UmbDeleteFolderEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.Script.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
	name: 'Script Folder Repository',
	api: () => import('./script-folder.repository.js'),
};

export const UMB_DELETE_SCRIPT_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Folder.Delete';

const entityActions = [
	{
		type: 'entityAction',
		alias: UMB_DELETE_SCRIPT_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Delete Script folder',
		api: UmbDeleteFolderEntityAction,
		forEntityTypes: [UMB_SCRIPT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-trash',
			label: 'Delete folder...',
			repositoryAlias: UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
		},
	},
];

export const manifests = [folderRepository, ...entityActions];
