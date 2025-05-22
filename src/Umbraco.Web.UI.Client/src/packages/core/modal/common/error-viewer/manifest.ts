export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ErrorViewer',
		name: 'Error Viewer Modal',
		element: () => import('./error-viewer-modal.element.js'),
	},
];
