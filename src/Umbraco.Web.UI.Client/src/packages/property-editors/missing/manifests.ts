export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Missing',
		name: 'Missing Property Editor UI',
		element: () => import('./property-editor-ui-missing.element.js'),
		meta: {
			label: 'Missing',
			propertyEditorSchemaAlias: undefined, // By setting it to undefined, this editor won't appear in the property editor UI picker modal.
			icon: 'icon-ordered-list',
			group: '',
			supportsReadOnly: true,
		},
	},
];
