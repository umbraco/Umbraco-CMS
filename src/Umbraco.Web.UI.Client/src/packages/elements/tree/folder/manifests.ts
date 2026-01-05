import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS } from './repository/constants.js';
import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from './entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'folderUpdate',
		alias: 'Umb.EntityAction.Element.Folder.Rename',
		name: 'Rename Element Folder Entity Action',
		forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: 'Umb.EntityAction.Element.Folder.Delete',
		name: 'Delete Element Folder Entity Action',
		forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
		},
	},
	...repositoryManifests,
	...workspaceManifests,
];
