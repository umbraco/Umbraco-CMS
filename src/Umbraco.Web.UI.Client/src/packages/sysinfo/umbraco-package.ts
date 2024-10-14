export const name = 'Umbraco.Core.Sysinfo';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Sysinfo Bundle',
		alias: 'Umb.Bundle.Sysinfo',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
