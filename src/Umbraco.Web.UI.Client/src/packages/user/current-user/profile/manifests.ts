import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_CURRENT_USER_ALLOW_CHANGE_PASSWORD_CONDITION_ALIAS } from '@umbraco-cms/backoffice/user';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'userProfileApp',
		alias: 'Umb.UserProfileApp.CurrentUser.Profile',
		name: 'Current User Profile User Profile App',
		element: () => import('./current-user-profile-user-profile-app.element.js'),
		weight: 900,
		meta: {
			label: 'Current User Profile User Profile App',
			pathname: 'profile',
		},
	},
	{
		type: 'currentUserAction',
		kind: 'default',
		alias: 'Umb.CurrentUser.Button.Edit',
		name: 'Current User Edit Button',
		weight: 1000,
		api: () => import('./edit-current-user.action.js'),
		meta: {
			label: '#general_edit',
			icon: 'edit',
		},
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: 'Umb.Section.Users',
			},
		],
	},
	{
		type: 'currentUserAction',
		kind: 'default',
		alias: 'Umb.CurrentUser.Button.ChangePassword',
		name: 'Current User Change Password Button',
		weight: 900,
		api: () => import('./change-password-current-user.action.js'),
		meta: {
			label: '#general_changePassword',
			icon: 'lock',
		},
		conditions: [
			{
				alias: UMB_CURRENT_USER_ALLOW_CHANGE_PASSWORD_CONDITION_ALIAS,
			},
		],
	},
];
