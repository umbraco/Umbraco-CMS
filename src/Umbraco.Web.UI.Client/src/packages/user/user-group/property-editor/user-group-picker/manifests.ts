export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.UserGroupPicker',
		name: 'User Group Picker Property Editor UI',
		element: () => import('./property-editor-ui-user-group-picker.element.js'),
		meta: {
			label: 'User Group Picker',
			icon: 'icon-users',
			group: '#propertyEditorUIGroups_people',
		},
	},
];
