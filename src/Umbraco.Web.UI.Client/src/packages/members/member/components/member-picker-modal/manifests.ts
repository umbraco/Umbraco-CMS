export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.MemberPicker',
		name: 'Member Picker Modal',
		element: () => import('./member-picker-modal.element.js'),
	},
];
