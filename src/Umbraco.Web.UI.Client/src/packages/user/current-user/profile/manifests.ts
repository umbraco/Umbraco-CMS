import { manifests as editProfileManifests } from './edit/manifests.js';
import { UMB_CURRENT_USER_ALLOW_CHANGE_PASSWORD_CONDITION_ALIAS } from '@umbraco-cms/backoffice/user';

const userProfileApp: UmbExtensionManifest = {
	type: 'userProfileApp',
	alias: 'Umb.UserProfileApp.CurrentUser.Profile',
	name: 'Current User Profile User Profile App',
	element: () => import('./current-user-profile-user-profile-app.element.js'),
	weight: 900,
	meta: {
		label: 'Current User Profile User Profile App',
		pathname: 'profile',
	},
};

const changePasswordAction: UmbExtensionManifest = {
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
};

export const manifests: Array<UmbExtensionManifest> = [userProfileApp, changePasswordAction, ...editProfileManifests];
