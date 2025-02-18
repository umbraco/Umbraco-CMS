import { UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS, manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const UMB_DELETE_PARTIAL_VIEW_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.PartialView.Folder.Delete';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: UMB_DELETE_PARTIAL_VIEW_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Delete Partial View folder Entity Action',
		forEntityTypes: [UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS,
		},
	},
	...repositoryManifests,
	...workspaceManifests,
];
