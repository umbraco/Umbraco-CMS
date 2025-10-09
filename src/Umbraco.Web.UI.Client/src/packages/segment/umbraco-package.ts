export const name = 'Umbraco.Core.Segment';
export const extensions = [
	{
		name: 'Segment Bundle',
		alias: 'Umb.Bundle.Segment',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
