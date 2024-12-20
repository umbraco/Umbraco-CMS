import type { UmbPasteClipboardEntryTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPasteClipboardEntryTranslator extends ManifestApi<UmbPasteClipboardEntryTranslator> {
	type: 'pasteClipboardEntryTranslator';
	forClipboardEntryTypes: string[];
}

declare global {
	interface UmbExtensionManifestMap {
		umbPasteClipboardEntryTranslator: ManifestPasteClipboardEntryTranslator;
	}
}
