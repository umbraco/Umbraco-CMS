export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Document User Permission Condition',
		alias: 'Umb.Condition.UserPermission.Document',
		api: () => import('./document-user-permission.condition.js'),
	},
];
