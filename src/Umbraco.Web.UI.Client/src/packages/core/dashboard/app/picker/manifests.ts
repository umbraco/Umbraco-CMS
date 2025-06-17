export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DashboardAppPicker',
		name: 'Umb Dashboard App Picker Modal',
		js: () => import('./picker-modal.element.js'),
	},
];
