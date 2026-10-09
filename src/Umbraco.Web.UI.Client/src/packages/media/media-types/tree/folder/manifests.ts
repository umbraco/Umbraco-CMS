import { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_TYPE_MENU_ITEM_ALIAS } from '../../menu/constants.js';
import { UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS } from './workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

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
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Media Type Folder Menu Structure Workspace Context',
		alias: 'Umb.Context.MediaTypeFolder.Menu.Structure',
		api: () => import('../../menu/media-type-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_MEDIA_TYPE_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.MediaTypeFolder.Breadcrumb',
		name: 'Media Type Folder Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	...repositoryManifests,
	...workspaceManifests,
];
