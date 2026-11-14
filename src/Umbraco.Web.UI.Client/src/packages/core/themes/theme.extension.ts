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
	 * A media query that defined when the theme applies, for example `(prefers-color-scheme: dark)`.
	 */
	mediaQuery?: string;
	/**
	 * The color scheme the theme presents. Defaults to `light`.
	 */
	colorScheme?: UmbThemeColorScheme;
}

export type UmbThemeColorScheme = 'light' | 'dark';

declare global {
	interface UmbExtensionManifestMap {
		UMB_THEME: ManifestTheme;
	}
}
