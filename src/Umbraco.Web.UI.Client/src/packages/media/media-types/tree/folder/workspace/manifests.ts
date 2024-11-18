import { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS,
		name: 'Media Type Folder Workspace',
		api: () => import('./media-type-folder-workspace.context.js'),
		meta: {
			entityType: UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.MediaType.Folder.Submit',
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
				match: UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
];
