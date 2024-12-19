import type { UmbClipboardPasteResolver } from './types.js';
import type { ManifestApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestClipboardPasteResolver
	extends ManifestApi<UmbClipboardPasteResolver>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'clipboardPasteResolver';
}

declare global {
	interface UmbExtensionManifestMap {
		umbClipboardPasteResolver: ManifestClipboardPasteResolver;
	}
}
