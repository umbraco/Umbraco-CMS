import { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/server';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'folderUpdate',
		alias: 'Umb.EntityAction.MediaType.Folder.Update',
		name: 'Rename Media Type Folder Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: 'Umb.EntityAction.MediaType.Folder.Delete',
		name: 'Delete Media Type Folder Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
	...repositoryManifests,
	...workspaceManifests,
];
