import { UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: 'Umb.Element.WorkspaceActionMenuItem.SchedulePublishing',
		name: 'Schedule publishing',
		weight: 20,
		api: () => import('./save-and-schedule.action.js'),
		forWorkspaceActions: 'Umb.WorkspaceAction.Element.SaveAndPublish',
		meta: {
			label: '#buttons_schedulePublish',
			icon: 'icon-globe',
		},
		conditions: [
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
