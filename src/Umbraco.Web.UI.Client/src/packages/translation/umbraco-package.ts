export const name = 'Umbraco.Core.TranslationManagement';
export const extensions = [
	{
		name: 'Translation Management Bundle',
		alias: 'Umb.Bundle.TranslationManagement',
		type: 'bundle',
		loader: () => import('./manifests.js'),
	},
];
