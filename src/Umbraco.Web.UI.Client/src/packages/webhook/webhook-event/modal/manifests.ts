export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Webhook.Events',
		name: 'Webhook Events Modal',
		js: () => import('./webhook-events-modal.element.js'),
	},
];
