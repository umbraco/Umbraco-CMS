export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow ExternalLogin Action Condition',
		alias: 'Umb.Condition.User.AllowExternalLoginAction',
		api: () => import('./user-allow-external-login-action.condition.js'),
	},
];
