export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow Delete Action Condition',
		alias: 'Umb.Condition.User.AllowDeleteAction',
		api: () => import('./user-allow-delete-action.condition.js'),
	},
];
