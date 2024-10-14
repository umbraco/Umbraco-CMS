export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow Disable Action Condition',
		alias: 'Umb.Condition.User.AllowDisableAction',
		api: () => import('./user-allow-disable-action.condition.js'),
	},
];
