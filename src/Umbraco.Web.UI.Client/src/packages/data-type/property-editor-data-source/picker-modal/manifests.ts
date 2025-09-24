export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.PropertyEditorDataSourcePicker',
		name: 'Property Editor Data Source Picker Modal',
		element: () => import('./picker-modal.element.js'),
	},
];
