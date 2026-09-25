export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.DocumentBlueprintStartNodeAccess',
		name: 'Document Blueprint Start Node Access Property Editor UI',
		element: () => import('./document-blueprint-start-node-access-property-editor-ui.element.js'),
		meta: {
			label: 'Document Blueprint Start Node Access',
			icon: 'icon-plugin',
			group: 'pickers',
			supportsReadOnly: true,
		},
	},
];
