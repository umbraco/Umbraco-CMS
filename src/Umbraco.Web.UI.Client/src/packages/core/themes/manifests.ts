import UmbThemeContext from './theme.context.js';
import { UMB_THEME_LIGHT_ALIAS, UMB_THEME_DARK_ALIAS, UMB_THEME_HIGH_CONTRAST_ALIAS } from './constants.js';
export { UMB_THEME_LIGHT_ALIAS, UMB_THEME_DARK_ALIAS, UMB_THEME_HIGH_CONTRAST_ALIAS };

export const themes: Array<UmbExtensionManifest> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.Theme',
		name: 'Theme Context',
		api: UmbThemeContext,
	},
	{
		type: 'theme',
		alias: UMB_THEME_LIGHT_ALIAS,
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
		alias: UMB_THEME_DARK_ALIAS,
		name: 'Dark (Experimental)',
		css: '/umbraco/backoffice/css/dark.css',
		weight: 200,
	},
	{
		type: 'theme',
		alias: UMB_THEME_HIGH_CONTRAST_ALIAS,
		name: 'High contrast (Experimental)',
		css: '/umbraco/backoffice/css/high-contrast.css',
		weight: 100,
	},
];

export const manifests = themes;
