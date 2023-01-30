import type { ManifestTheme } from '@umbraco-cms/models';

export const themes: Array<ManifestTheme> = [
	{
		type: 'theme',
		alias: 'umb-dark-theme',
		name: 'Dark',
		loader: () => import('./themes/dark.theme')
	},
];

export const manifests = themes;
