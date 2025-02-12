export const name = 'Umbraco.Core.Rte';
export const extensions = [
	{
		name: 'RTE Bundle',
		alias: 'Umb.Bundle.Rte',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
