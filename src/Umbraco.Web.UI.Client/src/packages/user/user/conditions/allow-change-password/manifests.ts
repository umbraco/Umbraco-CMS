export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow Change Password Condition',
		alias: 'Umb.Condition.User.AllowChangePassword',
		api: () => import('./user-allow-change-password-action.condition.js'),
	},
];
