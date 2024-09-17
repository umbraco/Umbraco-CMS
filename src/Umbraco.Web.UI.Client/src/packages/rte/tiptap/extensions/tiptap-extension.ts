import type { UmbTiptapExtensionBase } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTiptapExtension extends ManifestApi<UmbTiptapExtensionBase> {
	type: 'tiptapExtension';
}

declare global {
	interface UmbExtensionManifestMap {
		tiptapExtension: ManifestTiptapExtension;
	}
}
