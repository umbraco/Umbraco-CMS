export const themes: Array<UmbExtensionManifest> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.Theme',
		name: 'Theme Context',
		api: () => import('./theme.context.js'),
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
		name: 'Dark (Experimental)',
		css: '/umbraco/backoffice/css/dark.theme.css',
		weight: 200,
	},
	{
		type: 'theme',
		alias: 'umb-high-contrast-theme',
		name: 'High contrast (Experimental)',
		css: '/umbraco/backoffice/css/high-contrast.theme.css',
		weight: 100,
	},
];

export const manifests = themes;
