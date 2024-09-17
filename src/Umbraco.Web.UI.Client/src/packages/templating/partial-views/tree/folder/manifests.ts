import { UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE } from '../../entity.js';

export const UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Folder';
export const UMB_DELETE_PARTIAL_VIEW_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.PartialView.Folder.Delete';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS,
		name: 'Partial View Folder Repository',
		api: () => import('./partial-view-folder.repository.js'),
	},
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
];
