import type { ManifestTheme } from '@umbraco-cms/models';

export const themes: Array<ManifestTheme> = [
	{
		type: 'theme',
		alias: 'umb-light-theme',
		name: 'Light',
		weight: 300,
	},
	{
		type: 'theme',
		alias: 'umb-dark-theme',
		name: 'Dark',
		loader: () => import('./themes/dark.theme'),
		weight: 200,
	},
	{
		type: 'theme',
		alias: 'umb-high-contrast-theme',
		name: 'High contrast',
		loader: () => import('./themes/high-contrast.theme'),
		weight: 100,
	},
];

export const manifests = themes;
