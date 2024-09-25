import { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_FOLDER_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
		name: 'Media Type Folder Repository',
		api: () => import('./media-type-folder.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_MEDIA_TYPE_FOLDER_STORE_ALIAS,
		name: 'Media Type Folder Store',
		api: () => import('./media-type-folder.store.js'),
	},
	{
		type: 'entityAction',
		kind: 'folderUpdate',
		alias: 'Umb.EntityAction.MediaType.Folder.Update',
		name: 'Rename Media Type Folder Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
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
	},
];
