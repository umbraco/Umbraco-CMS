export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.PropertyEditorUiPicker',
		name: 'Property Editor UI Picker Modal',
		element: () => import('./property-editor-ui-picker-modal.element.js'),
	},
];
