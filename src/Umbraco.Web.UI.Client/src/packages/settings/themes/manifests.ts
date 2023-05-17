import type { ManifestTheme } from '@umbraco-cms/backoffice/extension-registry';

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
		css: 'src/backoffice/themes/themes/dark.theme.css',
		weight: 200,
	},
	{
		type: 'theme',
		alias: 'umb-high-contrast-theme',
		name: 'High contrast',
		css: 'src/backoffice/themes/themes/high-contrast.theme.css',
		weight: 100,
	},
];

export const manifests = themes;
