export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Sysinfo',
		name: 'Sysinfo Modal',
		js: () => import('./components/sysinfo.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.NewVersion',
		name: 'New Version Modal',
		js: () => import('./components/new-version.element.js'),
	},
];
