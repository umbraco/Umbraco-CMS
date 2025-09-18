export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.MissingPropertyEditor',
		name: 'Missing Property Editor Modal',
		element: () => import('./missing-editor-modal.element.js'),
	},
];
