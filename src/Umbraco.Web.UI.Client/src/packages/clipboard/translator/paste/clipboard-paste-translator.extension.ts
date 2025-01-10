import type { UmbClipboardPasteTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestClipboardPasteTranslator extends ManifestApi<UmbClipboardPasteTranslator> {
	type: 'clipboardPastePropertyValueTranslator';
	fromClipboardEntryValueType: string;
	toPropertyEditorUi: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbClipboardPasteTranslator: ManifestClipboardPasteTranslator;
	}
}
