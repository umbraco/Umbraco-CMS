export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Sysinfo',
		name: 'Sysinfo Modal',
		js: () => import('./components/sysinfo.element.js'),
	},
];
