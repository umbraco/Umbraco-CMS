import type { ManifestWithLoader } from '@umbraco-cms/backoffice/extensions-api';

// TODO: make or find type for JS Module with default export: Would be nice to support css file directly.

/**
 * Theme manifest for styling the backoffice of Umbraco such as dark, high contrast etc
 */
export interface ManifestTheme extends ManifestWithLoader<string> {
	type: 'theme';

	/**
	 * File location of the CSS file of the theme
	 *
	 * @examples ["themes/dark.theme.css"]
	 */
	css?: string;
}
