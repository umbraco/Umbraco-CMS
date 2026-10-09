export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.SectionPicker',
		name: 'Section Picker Property Editor UI',
		element: () => import('./property-editor-ui-section-picker.element.js'),
		meta: {
			label: 'Section Picker',
			icon: 'icon-grid',
			group: '#propertyEditorUIGroups_pickers',
		},
	},
];
