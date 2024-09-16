export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.UserGroupPicker',
		name: 'User Group Picker Modal',
		element: () => import('./user-group-picker/user-group-picker-modal.element.js'),
	},
];
