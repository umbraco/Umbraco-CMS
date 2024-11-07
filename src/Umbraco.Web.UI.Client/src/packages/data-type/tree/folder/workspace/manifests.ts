import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS,
		name: 'Data Type Folder Workspace',
		api: () => import('./data-type-folder-workspace.context.js'),
		meta: {
			entityType: UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.DataType.Folder.Submit',
		name: 'Submit Media Type Folder Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
];
