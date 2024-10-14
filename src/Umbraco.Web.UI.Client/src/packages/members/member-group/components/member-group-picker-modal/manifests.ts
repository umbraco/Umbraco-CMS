export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.MemberGroupPicker',
		name: 'Member Group Picker Modal',
		element: () => import('./member-group-picker-modal.element.js'),
	},
];
