export const UMB_THEME_LIGHT_ALIAS = 'umb-light-theme';
export const UMB_THEME_DARK_ALIAS = 'umb-dark-theme';
export const UMB_THEME_HIGH_CONTRAST_ALIAS = 'umb-high-contrast-theme';

export const themes: Array<UmbExtensionManifest> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.Theme',
		name: 'Theme Context',
		api: () => import('./theme.context.js'),
	},
	{
		type: 'theme',
		alias: UMB_THEME_LIGHT_ALIAS,
		name: 'Light',
		weight: 300,
	},
	{
		type: 'theme',
		alias: UMB_THEME_DARK_ALIAS,
		name: 'Dark (Experimental)',
		css: '/umbraco/backoffice/css/dark.theme.css',
		weight: 200,
	},
	{
		type: 'theme',
		alias: UMB_THEME_HIGH_CONTRAST_ALIAS,
		name: 'High contrast (Experimental)',
		css: '/umbraco/backoffice/css/high-contrast.theme.css',
		weight: 100,
	},
];

export const manifests = themes;
