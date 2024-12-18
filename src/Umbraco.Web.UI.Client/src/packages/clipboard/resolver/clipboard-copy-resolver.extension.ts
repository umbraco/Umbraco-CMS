import type { ManifestApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbClipboardCopyResolver } from './types';

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
