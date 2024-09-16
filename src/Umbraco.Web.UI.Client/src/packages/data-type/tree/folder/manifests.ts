import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
export const UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Folder';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
		name: 'Data Type Folder Repository',
		api: () => import('./data-type-folder.repository.js'),
	},
	{
		type: 'entityAction',
		kind: 'folderUpdate',
		alias: 'Umb.EntityAction.DataType.Folder.Rename',
		name: 'Rename Data Type Folder Entity Action',
		forEntityTypes: [UMB_DATA_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: 'Umb.EntityAction.DataType.Folder.Delete',
		name: 'Delete Data Type Folder Entity Action',
		forEntityTypes: [UMB_DATA_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
];
