export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.IconPicker',
		name: 'Icon Picker Property Editor UI',
		element: () => import('./property-editor-ui-icon-picker.element.js'),
		meta: {
			label: 'Icon Picker',
			icon: 'icon-autofill',
			group: 'common',
			settings: {
				properties: [
					{
						alias: 'placeholder',
						label: 'Placeholder icon (empty state)',
						description: 'Icon name to show when no icon is selected',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.IconPicker',
					},
					{
						alias: 'hideColors',
						label: 'Hide colors',
						description: 'Hide color swatches from modal',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
				],
			},
		},
	},
];
