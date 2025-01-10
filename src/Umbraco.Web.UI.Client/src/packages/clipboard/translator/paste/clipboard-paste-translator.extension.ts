import type { UmbClipboardPastePropertyValueTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestClipboardPasteTranslator extends ManifestApi<UmbClipboardPastePropertyValueTranslator> {
	type: 'clipboardPastePropertyValueTranslator';
	fromClipboardEntryValueType: string;
	toPropertyEditorUi: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbClipboardPasteTranslator: ManifestClipboardPasteTranslator;
	}
}
