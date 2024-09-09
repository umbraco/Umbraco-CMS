export const name = 'Umbraco.Core.TipTap';
export const extensions = [
	{
		name: 'TipTap Bundle',
		alias: 'Umb.Bundle.TipTap',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
