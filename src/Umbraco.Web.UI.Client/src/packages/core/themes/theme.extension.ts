import type { ManifestPlainCss } from '@umbraco-cms/backoffice/extension-api';
/**
 * Theme manifest for styling the backoffice of Umbraco such as dark, high contrast etc
 */
export interface ManifestTheme extends ManifestPlainCss {
	type: 'theme';
	meta?: MetaTheme;
}

export interface MetaTheme {
	/**
	 * A media query that the stylesheet only applies under, for example `(prefers-color-scheme: dark)`.
	 * When set, the theme follows the query live and its color scheme is `dark` while the query matches.
	 */
	mediaQuery?: string;
	/**
	 * The color scheme the theme presents when it has no `mediaQuery`. Defaults to `light`.
	 */
	colorScheme?: UmbThemeColorScheme;
}

export type UmbThemeColorScheme = 'light' | 'dark';

declare global {
	interface UmbExtensionManifestMap {
		UMB_THEME: ManifestTheme;
	}
}
