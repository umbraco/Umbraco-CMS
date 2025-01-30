import {
	UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '../../../user-permissions/constants.js';
import { UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: 'Umb.Document.WorkspaceActionMenuItem.PublishWithDescendants',
		name: 'Publish with descendants',
		weight: 10,
		api: () => import('./publish-with-descendants.action.js'),
		forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		meta: {
			label: '#buttons_publishDescendants',
			icon: 'icon-globe',
		},
		conditions: [
			{
				alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_DOCUMENT_UPDATE, UMB_USER_PERMISSION_DOCUMENT_PUBLISH],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
			{
				alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
