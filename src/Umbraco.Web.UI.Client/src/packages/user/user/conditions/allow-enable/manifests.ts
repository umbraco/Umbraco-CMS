export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow Enable Action Condition',
		alias: 'Umb.Condition.User.AllowEnableAction',
		api: () => import('./user-allow-enable-action.condition.js'),
	},
];
