export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Examine.FieldsSettings',
		name: 'Examine Field Settings Modal',
		element: () => import('./fields-settings/examine-fields-settings-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Examine.FieldsViewer',
		name: 'Examine Field Viewer Modal',
		element: () => import('./fields-viewer/examine-fields-viewer-modal.element.js'),
	},
];
