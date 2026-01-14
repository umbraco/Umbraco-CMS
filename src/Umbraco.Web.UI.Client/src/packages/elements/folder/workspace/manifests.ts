import { UMB_ELEMENT_ROOT_WORKSPACE_ALIAS } from '../../workspace/element-root/constants.js';
import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS,
		name: 'Element Folder Workspace',
		api: () => import('./element-folder-workspace.context.js'),
		meta: {
			entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.Element.Collection',
		name: 'Element Collection Workspace View',
		meta: {
			label: 'Folder',
			pathname: 'folder',
			icon: 'icon-folder',
			collectionAlias: 'Umb.Collection.Element',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_ELEMENT_ROOT_WORKSPACE_ALIAS, UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Element.Folder.Submit',
		name: 'Submit Element Folder Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
];
