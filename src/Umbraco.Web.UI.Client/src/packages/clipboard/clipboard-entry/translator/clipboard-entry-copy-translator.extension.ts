import type { UmbClipboardEntryCopyTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestClipboardEntryCopyTranslator extends ManifestApi<UmbClipboardEntryCopyTranslator> {
	type: 'clipboardEntryCopyTranslator';
	forClipboardEntryTypes: string[];
}

declare global {
	interface UmbExtensionManifestMap {
		umbClipboardEntryCopyTranslator: ManifestClipboardEntryCopyTranslator;
	}
}
