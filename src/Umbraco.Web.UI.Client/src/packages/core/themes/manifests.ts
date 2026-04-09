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
	/*
	 * The CSS paths below reference theme files provided by @umbraco-ui/uui.
	 * These files are copied into /umbraco/backoffice/css/ during build by:
	 *   - vite.config.ts (dev server): copies node_modules/@umbraco-ui/uui/dist/themes/*
	 *   - src/external/uui/vite.config.ts (production build): copies the same files into dist-cms/css/
	 *
	 * If UUI adds, removes, or renames theme files, update the entries here to match.
	 */
	{
		type: 'theme',
		alias: 'umb-dark-theme',
		name: 'Dark (Experimental)',
		css: '/umbraco/backoffice/css/dark.css',
		weight: 200,
	},
	{
		type: 'theme',
		alias: 'umb-high-contrast-theme',
		name: 'High contrast (Experimental)',
		css: '/umbraco/backoffice/css/high-contrast.css',
		weight: 100,
	},
];

export const manifests = themes;
