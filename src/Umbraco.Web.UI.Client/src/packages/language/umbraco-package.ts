export const name = 'Umbraco.Core.Language';
export const extensions = [
	{
		name: 'Language Bundle',
		alias: 'Umb.Bundle.Language',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
