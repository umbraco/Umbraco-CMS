import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

/**
 * Preview apps are displayed in the menu of the preview window.
 */
export interface ManifestPreviewAppProvider extends ManifestElement {
	type: 'previewApp';
}

declare global {
	interface UmbExtensionManifestMap {
		UmbPreviewAppProviderExtension: ManifestPreviewAppProvider;
	}
}
