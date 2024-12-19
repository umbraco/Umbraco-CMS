import type { UmbClipboardCopyResolver } from './types.js';
import type { ManifestApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestClipboardCopyResolver
	extends ManifestApi<UmbClipboardCopyResolver>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'clipboardCopyResolver';
}

declare global {
	interface UmbExtensionManifestMap {
		umbClipboardCopyResolver: ManifestClipboardCopyResolver;
	}
}
