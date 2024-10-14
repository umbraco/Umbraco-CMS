export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow Mfa Action Condition',
		alias: 'Umb.Condition.User.AllowMfaAction',
		api: () => import('./user-allow-mfa-action.condition.js'),
	},
];
