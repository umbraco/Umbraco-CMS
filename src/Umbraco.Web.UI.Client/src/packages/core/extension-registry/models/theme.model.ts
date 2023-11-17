import type { ManifestPlainCss } from '@umbraco-cms/backoffice/extension-api';
/**
 * Theme manifest for styling the backoffice of Umbraco such as dark, high contrast etc
 */
export interface ManifestTheme extends ManifestPlainCss<string> {
	type: 'theme';
}
