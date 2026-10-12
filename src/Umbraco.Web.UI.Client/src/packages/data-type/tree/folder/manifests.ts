import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_DATA_TYPE_MENU_ITEM_ALIAS } from '../../menu/constants.js';
import { UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS } from './workspace/index.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
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
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Data Type Folder Menu Structure Workspace Context',
		alias: 'Umb.Context.DataTypeFolder.Menu.Structure',
		api: () => import('../../menu/data-type-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_DATA_TYPE_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.DataTypeFolder.Breadcrumb',
		name: 'Data Type Folder Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	...repositoryManifests,
	...workspaceManifests,
];
