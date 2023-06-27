import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const themes: Array<ManifestTypes> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.Theme',
		name: 'Theme Context',
		loader: () => import('./theme.context.js'),
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
		css: 'src/packages/settings/themes/themes/dark.theme.css',
		weight: 200,
	},
	{
		type: 'theme',
		alias: 'umb-high-contrast-theme',
		name: 'High contrast',
		css: 'src/packages/settings/themes/themes/high-contrast.theme.css',
		weight: 100,
	},
];

export const manifests = themes;
