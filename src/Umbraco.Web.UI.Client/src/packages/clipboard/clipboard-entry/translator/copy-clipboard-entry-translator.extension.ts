import type { UmbCopyClipboardEntryTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestCopyClipboardEntryTranslator extends ManifestApi<UmbCopyClipboardEntryTranslator> {
	type: 'copyClipboardEntryTranslator';
	forClipboardEntryTypes: string[];
}

declare global {
	interface UmbExtensionManifestMap {
		umbCopyClipboardEntryTranslator: ManifestCopyClipboardEntryTranslator;
	}
}
