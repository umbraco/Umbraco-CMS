export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Info',
		name: 'Info Modal',
		element: () => import('./info-modal.element.js'),
	},
];
