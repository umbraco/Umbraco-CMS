export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Missing',
		name: 'Missing Property Editor UI',
		element: () => import('./property-editor-ui-missing.element.js'),
		meta: {
			label: 'Missing Property Editor',
			propertyEditorSchemaAlias: undefined, // By setting it to undefined, this editor won't appear in the property editor UI picker modal.
			icon: 'icon-circle-dotted',
			group: '',
			supportsReadOnly: true,
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.MissingUi',
		name: 'Missing Property Editor UI UI',
		element: () => import('./property-editor-ui-missing-ui.element.js'),
		meta: {
			label: 'Missing Property Editor UI',
			propertyEditorSchemaAlias: undefined, // By setting it to undefined, this editor won't appear in the property editor UI picker modal.
			icon: 'icon-circle-dotted',
			group: '',
			supportsReadOnly: true,
		},
	},
];
