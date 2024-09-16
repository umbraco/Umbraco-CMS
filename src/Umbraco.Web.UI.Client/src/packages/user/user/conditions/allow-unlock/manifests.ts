export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow Unlock Action Condition',
		alias: 'Umb.Condition.User.AllowUnlockAction',
		api: () => import('./user-allow-unlock-action.condition.js'),
	},
];
