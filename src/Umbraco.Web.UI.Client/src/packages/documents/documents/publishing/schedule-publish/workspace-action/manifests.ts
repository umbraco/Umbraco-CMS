import {
	UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '../../../user-permissions/constants.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: 'Umb.Document.WorkspaceActionMenuItem.SchedulePublishing',
		name: 'Schedule publishing',
		weight: 20,
		api: () => import('./save-and-schedule.action.js'),
		forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		meta: {
			label: '#buttons_schedulePublish',
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
		],
	},
];
