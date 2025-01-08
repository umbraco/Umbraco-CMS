import { UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_DELETE_SCRIPT_FOLDER_ENTITY_ACTION_ALIAS } from './constants.js';
import { UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
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
	...repositoryManifests,
	...workspaceManifests,
];
