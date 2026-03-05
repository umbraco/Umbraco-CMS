import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_UNPUBLISH,
	UMB_USER_PERMISSION_ELEMENT_UPDATE,
} from '../../../user-permissions/constants.js';
import { UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: 'Umb.Element.WorkspaceActionMenuItem.Unpublish',
		name: 'Unpublish Element',
		weight: 0,
		api: () => import('./unpublish.action.js'),
		forWorkspaceActions: 'Umb.WorkspaceAction.Element.SaveAndPublish',
		meta: {
			label: '#actions_unpublish',
			icon: 'icon-globe',
		},
		conditions: [
			{
				alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_ELEMENT_UPDATE, UMB_USER_PERMISSION_ELEMENT_UNPUBLISH],
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
