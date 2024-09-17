export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.WebhookRoot',
		name: 'Webhook Root Workspace',
		element: () => import('./webhook-root-workspace.element.js'),
		meta: {
			entityType: 'webhook-root',
		},
	},
];
