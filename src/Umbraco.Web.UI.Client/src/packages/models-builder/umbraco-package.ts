export const name = 'Umbraco.Core.ModelsBuilder';
export const extensions = [
	{
		name: 'Models Builder Management Bundle',
		alias: 'Umb.Bundle.ModelsBuilder',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
