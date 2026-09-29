export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Sysinfo',
		name: 'Sysinfo Modal',
		js: () => import('./components/sysinfo.element.js'),
	},
];

export const name = 'Umbraco.Core.Sysinfo';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Sysinfo Bundle',
		alias: 'Umb.Bundle.Sysinfo',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
