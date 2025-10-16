export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'picker',
		alias: 'Umb.PropertyEditorDataSource.WebhookPicker',
		name: 'Webhook Picker Data Source',
		api: () => import('./example-webhook-picker-data-source.js'),
		meta: {
			label: 'Webhooks',
			icon: 'icon-webhook',
			description: 'Pick a webhook',
		},
	},
];
