export const name = 'Umbraco.Core.ExtensionInsight';
export const extensions = [
	{
		name: 'Extension Insight Bundle',
		alias: 'Umb.Bundle.ExtensionInsight',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
