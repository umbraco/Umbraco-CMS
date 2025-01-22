import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../../../workspace/constants.js';
import {
	UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '../../../user-permissions/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		name: 'Save And Publish Document Workspace Action',
		weight: 70,
		api: () => import('./save-and-publish.action.js'),
		meta: {
			label: '#buttons_saveAndPublish',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_DOCUMENT_UPDATE, UMB_USER_PERMISSION_DOCUMENT_PUBLISH],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
