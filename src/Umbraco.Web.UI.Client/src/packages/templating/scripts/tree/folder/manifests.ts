import { UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

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
		kind: 'folderDelete',
		alias: UMB_DELETE_SCRIPT_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Delete Script folder',
		forEntityTypes: [UMB_SCRIPT_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
		},
	},
];

export const manifests: Array<ManifestTypes> = [folderRepository, ...entityActions];
