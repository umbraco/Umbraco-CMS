import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS } from './repository/constants.js';
import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from './entity.js';
import { UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS } from './workspace/constants.js';
import { UMB_DOCUMENT_TYPE_MENU_ITEM_ALIAS } from '../../menu/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'folderUpdate',
		alias: 'Umb.EntityAction.DocumentType.Folder.Rename',
		name: 'Rename Document Type Folder Entity Action',
		forEntityTypes: [UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: 'Umb.EntityAction.DocumentType.Folder.Delete',
		name: 'Delete Document Type Folder Entity Action',
		forEntityTypes: [UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Document Type Folder Menu Structure Workspace Context',
		alias: 'Umb.Context.DocumentTypeFolder.Menu.Structure',
		api: () => import('../../menu/document-type-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_DOCUMENT_TYPE_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.DocumentTypeFolder.Breadcrumb',
		name: 'Document Type Folder Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	...repositoryManifests,
	...workspaceManifests,
];
