import { UMB_MEMBER_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import {
	UMB_WORKSPACE_CONDITION_ALIAS,
	UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Member Profile Data Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Member.ProfileData',
		element: () => import('./member-profile-data-workspace-info-app.element.js'),
		// Higher weight surfaces the box above "Referenced by" (which has no explicit weight).
		weight: 100,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
