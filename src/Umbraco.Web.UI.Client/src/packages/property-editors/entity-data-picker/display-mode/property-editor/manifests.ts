export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.EntityDataPicker.DisplayMode',
		name: 'Entity Data Picker Display Mode Property Editor UI',
		element: () => import('./entity-data-picker-display-mode-property-editor-ui.element.js'),
		meta: {
			label: 'Entity Data Picker Display Mode',
			icon: 'icon-page-add',
			group: 'pickers',
			supportsReadOnly: true,
		},
	},
];
