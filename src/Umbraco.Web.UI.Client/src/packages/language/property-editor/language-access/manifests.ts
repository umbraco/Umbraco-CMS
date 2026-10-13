export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.LanguageAccess',
		name: 'Language Access Property Editor UI',
		element: () => import('./language-access-property-editor-ui.element.js'),
		meta: {
			label: 'Language Access',
			icon: 'icon-globe',
			group: '#propertyEditorUIGroups_pickers',
			supportsReadOnly: true,
		},
	},
];
