import { UmbChangePasswordCurrentUserAction } from './change-password-current-user.action.js';
import { UmbEditCurrentUserAction } from './edit-current-user.action.js';
import type {
	ManifestCurrentUserActionDefaultKind,
	ManifestUserProfileApp,
} from '@umbraco-cms/backoffice/extension-registry';

const userProfileApps: Array<ManifestUserProfileApp> = [
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
];

const currentUserActions: Array<ManifestCurrentUserActionDefaultKind> = [
	{
		type: 'currentUserAction',
		kind: 'default',
		alias: 'Umb.CurrentUser.Button.Edit',
		name: 'Current User Edit Button',
		weight: 1000,
		api: UmbEditCurrentUserAction,
		meta: {
			label: '#general_edit',
			icon: 'edit',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionUserPermission',
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
		api: UmbChangePasswordCurrentUserAction,
		meta: {
			label: '#general_changePassword',
			icon: 'lock',
		},
	},
];

export const manifests = [...userProfileApps, ...currentUserActions];
