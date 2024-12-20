import type { UmbClipboardEntryPasteTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestClipboardEntryPasteTranslator extends ManifestApi<UmbClipboardEntryPasteTranslator> {
	type: 'clipboardEntryPasteTranslator';
	forClipboardEntryTypes: string[];
	forPropertyEditorUiAliases: string[];
}

declare global {
	interface UmbExtensionManifestMap {
		umbClipboardEntryPasteTranslator: ManifestClipboardEntryPasteTranslator;
	}
}
