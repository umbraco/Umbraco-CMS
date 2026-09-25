import { UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEMBER_TYPE_MENU_ITEM_ALIAS } from '../../menu/constants.js';
import { UMB_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { UMB_MEMBER_TYPE_FOLDER_WORKSPACE_ALIAS } from './workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

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
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Member Type Folder Menu Structure Workspace Context',
		alias: 'Umb.Context.MemberTypeFolder.Menu.Structure',
		api: () => import('../../menu/member-type-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_MEMBER_TYPE_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.MemberTypeFolder.Breadcrumb',
		name: 'Member Type Folder Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	...repositoryManifests,
	...workspaceManifests,
];
