import { UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'folderUpdate',
		alias: 'Umb.EntityAction.MemberType.Folder.Update',
		name: 'Rename Member Type Folder Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: 'Umb.EntityAction.MemberType.Folder.Delete',
		name: 'Delete Member Type Folder Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
	...repositoryManifests,
	...workspaceManifests,
];
