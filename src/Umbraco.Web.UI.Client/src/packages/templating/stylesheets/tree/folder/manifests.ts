import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const UMB_DELETE_STYLESHEET_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Stylesheet.Folder.Delete';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: UMB_DELETE_STYLESHEET_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Delete Stylesheet folder Entity Action',
		forEntityTypes: [UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
		},
	},
	...repositoryManifests,
	...workspaceManifests,
];
