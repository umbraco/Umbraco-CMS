import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const themes: Array<ManifestTypes> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.Theme',
		name: 'Theme Context',
		js: () => import('./theme.context.js'),
	},
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
		css: 'src/css/themes/dark.theme.css',
		weight: 200,
	},
	{
		type: 'theme',
		alias: 'umb-high-contrast-theme',
		name: 'High contrast',
		css: 'src/css/themes/high-contrast.theme.css',
		weight: 100,
	},
];

export const manifests = themes;
